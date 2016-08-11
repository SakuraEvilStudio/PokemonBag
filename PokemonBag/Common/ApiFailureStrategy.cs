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

        public async Task<ApiOperation> HandleApiFailure()
        {
            return ApiOperation.Abort;
        }

        public void HandleApiSuccess()
        {
        }

        public async Task<ApiOperation> HandleApiFailure(RequestEnvelope request, ResponseEnvelope response)
        {
            return ApiOperation.Abort;
        }

        public void HandleApiSuccess(RequestEnvelope request, ResponseEnvelope response)
        {
        }

    }
}
