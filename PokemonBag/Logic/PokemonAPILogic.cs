using Google.Protobuf;
using PokemonBag.State;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonBag.Logic
{
    public class PokemonAPILogic
    {
        public async Task<IResult> Login(ISession session)
        {
            IResult checkResult = await CheckLogin(session);

            if (checkResult.IsSuccess == false)
                return checkResult;

            try
            {
                if (session.Settings.AuthType == AuthType.Google || session.Settings.AuthType == AuthType.Ptc)
                {
                    await session.Client.Login.DoLogin();
                }
                else
                {
                    return new LoginResult { IsSuccess = false, Message = "Unknown AuthType" };
                }
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten().InnerException;
            }
            catch (LoginFailedException)
            {
                return new LoginResult { IsSuccess = false, Message = "User credentials are invalid and login failed" };

            }
            catch (Exception ex) when (ex is PtcOfflineException || ex is AccessTokenExpiredException)
            {
                return new LoginResult { IsSuccess = false, Message = "PTC Servers are probably down OR your credentials are wrong. Try google" };
            }
            catch (AccountNotVerifiedException)
            {
                return new LoginResult { IsSuccess = false, Message = "Account not verified!Exiting..." };
            }
            catch (GoogleException e)
            {
                return new LoginResult { IsSuccess = false, Message = "Make sure you have entered the right Email & Password." };
            }
            catch (InvalidProtocolBufferException ex) when (ex.Message.Contains("SkipLastField"))
            {
                return new LoginResult { IsSuccess = false, Message = "Connection refused. Your IP might have been Blacklisted by Niantic. Exiting.." };

                }
            catch (Exception e)
            {
                return new LoginResult { IsSuccess = false, Message = e.ToString() };
            }

            await DownloadProfile(session);
            if (session.Profile == null)
            {
                return new LoginResult { IsSuccess = false, Message = "Login failure!" }; ;
            }
            return new LoginResult { IsSuccess = true, Message = "" }; ;
        }

        private static async Task<IResult> CheckLogin(ISession session)
        {

            if (session.Settings.AuthType == AuthType.Google &&
                (session.Settings.GoogleUsername == null || session.Settings.GooglePassword == null))
            {
                return new LoginResult { IsSuccess = false, Message = "You need to fill out Username and Password!" };
            }
            else if (session.Settings.AuthType == AuthType.Ptc &&
                     (session.Settings.PtcUsername == null || session.Settings.PtcPassword == null))
            {
                return new LoginResult { IsSuccess = false, Message = "You need to fill out Username and Password! " };
            }
            return  new LoginResult { IsSuccess = true, Message = "" }; ;
        }

        public async Task<IResult> DownloadProfile(ISession session)
        {
            try
            {
                session.Profile = await session.Client.Player.GetPlayer();
                return new LoginResult { IsSuccess = true, Message = "" }; ;
            }
            catch (System.UriFormatException e)
            {
                return new LoginResult { IsSuccess = false, Message = e.ToString() }; ;
            }
            catch (Exception ex)
            {
                return new LoginResult { IsSuccess = false, Message = ex.ToString() }; ;
            }
        }
    }
}