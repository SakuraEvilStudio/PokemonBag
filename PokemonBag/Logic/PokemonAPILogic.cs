using Google.Protobuf;
using PokemonBag.State;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            IResult checkLoginResult = await CheckLogin(session);
            if (checkLoginResult.IsSuccess == false)
                        return checkLoginResult;
            try
            {
                await session.Client.Login.DoLogin();
            }
            catch (PtcOfflineException)
            {
                await Task.Delay(20000);
                return new LoginResult { IsSuccess = false, Message = "PTC Servers are probably down OR your credentials are wrong. Try google" };
            }

            catch (AccessTokenExpiredException)
            {
                await Task.Delay(2000);
                return new LoginResult { IsSuccess = false, Message = "PTC Servers are probably down OR your credentials are wrong. Try google" };
            }
            catch (InvalidResponseException)
            {
 
                return new LoginResult { IsSuccess = false, Message = "Niantic Servers unstable, throttling API Calls." };
            }
            catch (AccountNotVerifiedException)
            {
                await Task.Delay(2000);
                return new LoginResult { IsSuccess = false, Message = "Account not verified!" };
            }
            catch (GoogleException e)
            {
                await Task.Delay(2000);
                return new LoginResult { IsSuccess = false, Message = "Make sure you have entered the right Email & Password." };
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
                await Task.Delay(2000);
                return new LoginResult { IsSuccess = false, Message = "You need to fill out Username and Password!" };
            }
            else if (session.Settings.AuthType == AuthType.Ptc &&
                     (session.Settings.PtcUsername == null || session.Settings.PtcPassword == null))
            {
                await Task.Delay(2000);
                return new LoginResult { IsSuccess = false, Message = "You need to fill out Username and Password! " };
            }
            return new LoginResult { IsSuccess = true, Message = "" }; ;
        }

        public async Task DownloadProfile(ISession session)
        {
            session.Profile = await session.Client.Player.GetPlayer();
        }

    }
}