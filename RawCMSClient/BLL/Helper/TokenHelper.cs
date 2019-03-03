﻿using RawCMSClient.BLL.Core;
using RawCMSClient.BLL.Model;
using RawCMSClient.BLL.Parser;
using RestSharp;
using System;
using System.IO;

namespace RawCMSClient.BLL.Helper
{
    public   class TokenHelper
    {

        private static Runner log = LogProvider.Runner;

        public static string getToken(LoginOptions opts)
        {

            //string baseUrl = ClientConfig.GetValue<string>("BaseUrl");
            
            var url = $"{opts.ServerUrl}/connect/token";

            log.Debug($"Server url: {url}");

            //create RestSharp client and POST request object
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);

            //add GetToken() API method parameters
            request.Parameters.Clear();
            request.AddParameter("grant_type", "password");

            request.AddParameter("username", opts.Username);
            request.AddParameter("password", opts.Password);

            request.AddParameter("client_id", opts.ClientId);
            request.AddParameter("client_secret", opts.ClientSecret);
            request.AddParameter("scoope", "openid");


            //make the API request and get the response
            IRestResponse response = client.Execute(request);
            TokenResponse res = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(response.Content);
            if (response.IsSuccessful)
            {
                log.Debug("Success response token");          
                return res.access_token;
            }
            else
            {

               log.Warn("Unable to get valid token.");
                throw new ExceptionToken(res.error, res.error_description);
            }
            
        }

        public static void SaveTokenToFile(string filePath,ConfigFile cf)
        {
            log.Debug("Save config to file...");
            
            log.Debug($"FilePath: {filePath}");
            
            try
            {
                using (StreamWriter outputFile = new StreamWriter(filePath))
                {
                    outputFile.Write(cf.ToString());
                }
            }
            catch (Exception e)
            {
                log.Error("The file could not be writed:", e );
                
            }

        }

        public static string getTokenFromFile()
        {
            string token = string.Empty;
            log.Debug("get token from file...");

            string filePath = Environment.GetEnvironmentVariable("RAWCMSCONFIG",EnvironmentVariableTarget.Process);
            log.Debug($"Config file: {filePath}");

            if (string.IsNullOrEmpty(filePath))
            {
                log.Warn("Config file not found. Perform login.");
                return null;
            }

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(filePath))
                {
                    var data = sr.ReadToEnd();
                    ConfigFile config = new ConfigFile(data);
                    token = config.Token;
                    log.Debug($"Get token From file:\n---- TOKEN ------\n{token}\n-----------------");
                }
            }
            catch (Exception e)
            {
                log.Error("The file could not be read:",e);
                
            }
            return token;
        }
    }
}
