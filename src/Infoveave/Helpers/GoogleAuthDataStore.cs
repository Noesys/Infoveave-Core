/* Copyright © 2015-2016 Noesys Software Pvt.Ltd. - All Rights Reserved
 * -------------
 * This file is part of Infoveave.
 * Infoveave is dual licensed under Infoveave Commercial License and AGPL v3  
 * -------------
 * You should have received a copy of the GNU Affero General Public License v3
 * along with this program (Infoveave)
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the Infoveave without
 * disclosing the source code of your own applications.
 * -------------
 * Authors: Naresh Jois <naresh@noesyssoftware.com>, et al.
 */
#if NET461
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
#pragma warning disable CS1591
namespace Infoveave.Helpers
{
    public static class GoogleAuth
    {
        public static ClientSecrets AuthInfo = new ClientSecrets
        {
            ClientId = "588082001949-kk92h9umheug5v49668liacq81sgh3bk.apps.googleusercontent.com",
            ClientSecret = "uGJdQFl4tx6lOTWOGvkjdSkM"
        };

        public static async Task<UserCredential> GetAuthorisation(string path, string tenant, string uniqueIdentifier, CancellationToken cancellationToken)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = GoogleAuth.AuthInfo,
                Scopes = new[] { Google.Apis.Oauth2.v2.Oauth2Service.Scope.UserinfoEmail, Google.Apis.Oauth2.v2.Oauth2Service.Scope.UserinfoProfile, Google.Apis.Analytics.v3.AnalyticsService.Scope.AnalyticsReadonly },
                DataStore = new GoogleAuthDataStore(tenant, path),
            });
            var authResult = await new GoogleAuthApp(flow, uniqueIdentifier, null, null).AuthorizeAsync(cancellationToken);
            return authResult.Credential;
        }
    }


    public class GoogleAuthApp : AuthorizationCodeWebApp
    {
        private IAuthorizationCodeFlow _flow;
        private string _uniqueIdentifier;
        public GoogleAuthApp(IAuthorizationCodeFlow flow,
            string uniqueIdentifier,
            string callbackURL,
            string currentURL
            ) : base(flow, callbackURL, currentURL)
        {
            this._flow = flow;
            this._uniqueIdentifier = uniqueIdentifier;
        }
        public Task<AuthResult> AuthorizeAsync(CancellationToken taskCancellationToken)
        {
            return base.AuthorizeAsync(this._uniqueIdentifier, taskCancellationToken);
        }
    }
    public class GoogleConnectInfo
    {
        public string UniqueIdentifier { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public string TempKey { get; set; }
        public string TokenKey { get; set; }
    }

    public class OrganisationGoogleConnectStore
    {
        public async Task<string> AddAccount(string path, string tenant)
        {
            if (!System.IO.File.Exists(path + "/GoogleAccounts-" + tenant + ".json"))
            {
                System.IO.File.WriteAllText(path + "/GoogleAccounts-" + tenant + ".json", "[]");
            }
            var connections = JsonConvert.DeserializeObject<List<GoogleConnectInfo>>(System.IO.File.ReadAllText(path + "/GoogleAccounts-" + tenant + ".json"));
            var newConnection = new GoogleConnectInfo
            {
                UniqueIdentifier = Guid.NewGuid().ToString()
            };
            connections.Add(newConnection);
            var content = JsonConvert.SerializeObject(connections);
            System.IO.File.WriteAllText(path + "/GoogleAccounts-" + tenant + ".json", content);
            await Task.Delay(0);
            return newConnection.UniqueIdentifier;
        }

        public async Task<bool> UpdateAccount(string path, string tenant, string UniqueIdentifier, string username, string email)
        {
            await Task.Delay(0);
            var connections = JsonConvert.DeserializeObject<List<GoogleConnectInfo>>(System.IO.File.ReadAllText(path + "/GoogleAccounts-" + tenant + ".json"));
            var existing = connections.FirstOrDefault(c => c.Email == email);
            if (existing != null)
            {
                connections.Remove(connections.First(c => c.UniqueIdentifier == UniqueIdentifier));
            }
            else
            {
                var connection = connections.First(c => c.UniqueIdentifier == UniqueIdentifier);
                connection.Email = email;
                connection.UserName = username;
            }
            System.IO.File.WriteAllText(path + "/GoogleAccounts-" + tenant + ".json", JsonConvert.SerializeObject(connections));
            return true;
        }

        public async Task<List<GoogleConnectInfo>> GetAllAccounts(string path, string tenant)
        {
            if (!System.IO.File.Exists(path + "/GoogleAccounts-" + tenant + ".json"))
            {
                System.IO.File.WriteAllText(path + "/GoogleAccounts-" + tenant + ".json", "[]");
            }
            var connections = JsonConvert.DeserializeObject<List<GoogleConnectInfo>>(System.IO.File.ReadAllText(path + "/GoogleAccounts-" + tenant + ".json"));
            connections = connections.Where(c => !string.IsNullOrEmpty(c.TokenKey)).ToList();
            await Task.Delay(0);
            return connections;
        }
    }


    public class GoogleAuthDataStore : Google.Apis.Util.Store.IDataStore
    {
        private string _tenant;
        private string _folderPath;
        public GoogleAuthDataStore(string tenant, string folderPath)
        {
            _tenant = tenant;
            _folderPath = folderPath;
        }
        public Task ClearAsync()
        {
            System.IO.File.WriteAllText(_folderPath + "/GoogleAccounts-" + _tenant + ".json", "[]");
            return Task.Delay(0);
        }

        public Task DeleteAsync<T>(string key)
        {
            var connections = JsonConvert.DeserializeObject<List<GoogleConnectInfo>>(System.IO.File.ReadAllText(_folderPath + "/GoogleAccounts-" + _tenant + ".json"));
            var connection = connections.First(c => c.UniqueIdentifier == key.Replace("oauth_", ""));
            if (typeof(T) == typeof(string))
            {
                connection.TempKey = string.Empty;
            }
            else
            {
                connection.TokenKey = string.Empty;
            }
            System.IO.File.WriteAllText(_folderPath + "/GoogleAccounts-" + _tenant + ".json", JsonConvert.SerializeObject(connections));
            return Task.Delay(0);
        }

        public Task<T> GetAsync<T>(string key)
        {
            var connections = JsonConvert.DeserializeObject<List<GoogleConnectInfo>>(System.IO.File.ReadAllText(_folderPath + "/GoogleAccounts-" + _tenant + ".json"));
            var connection = connections.First(c => c.UniqueIdentifier == key.Replace("oauth_", ""));
            if (connection == null || string.IsNullOrEmpty(connection.TokenKey))
            {
                return Task.FromResult(default(T));
            }
            dynamic result;
            if (typeof(T) == typeof(string))
            {
                result = connection.TempKey;
            }
            else
            {
                result = JsonConvert.DeserializeObject<Google.Apis.Auth.OAuth2.Responses.TokenResponse>(connection.TokenKey);
            };
            return Task.FromResult(result);
        }

        public Task StoreAsync<T>(string key, T value)
        {
            var connections = JsonConvert.DeserializeObject<List<GoogleConnectInfo>>(System.IO.File.ReadAllText(_folderPath + "/GoogleAccounts-" + _tenant + ".json"));
            var connection = connections.First(c => c.UniqueIdentifier == key.Replace("oauth_", ""));
            if (typeof(T) == typeof(string))
            {
                connection.TempKey = value.ToString();
            }
            else
            {
                connection.TokenKey = JsonConvert.SerializeObject(value);
            }
            System.IO.File.WriteAllText(_folderPath + "/GoogleAccounts-" + _tenant + ".json", JsonConvert.SerializeObject(connections));
            return Task.Delay(0);
        }
    }
}
#pragma warning restore CS1591
#endif
