using System;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Networking.Envelopes;
using PokemonBag.State;

namespace PokemonBag.Common
{
    public class ApiFailureStrategy : IApiFailureStrategy
    {
        private readonly ISession _session;
        private int _retryCount;

        public ApiFailureStrategy(ISession session)
        {
            _session = session;
        }

        private async void DoLogin()
        {
            try
            {
                await _session.Client.Login.DoLogin();
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten().InnerException;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<ApiOperation> HandleApiFailure(RequestEnvelope request, ResponseEnvelope response)
        {
            if (_retryCount == 11)
                return ApiOperation.Abort;

            await Task.Delay(500);
            _retryCount++;

            if (_retryCount % 5 == 0)
            {
                try
                {
                    DoLogin();
                }
                catch (PtcOfflineException)
                {
                    await Task.Delay(20000);
                }
                catch (AccessTokenExpiredException)
                {

                    await Task.Delay(2000);
                }
                catch (Exception ex) when (ex is InvalidResponseException || ex is TaskCanceledException)
                {
                    await Task.Delay(1000);
                }
            }

            return ApiOperation.Retry;
        }

        public void HandleApiSuccess(RequestEnvelope request, ResponseEnvelope response)
        {
            _retryCount = 0;
        }
    }
}
