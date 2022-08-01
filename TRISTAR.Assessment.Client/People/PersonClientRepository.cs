using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;
using TRISTAR.Assessment.Infrastructure;

namespace TRISTAR.Assessment.People
{
    /// <summary>
    /// This is the client-side repository for people. 
    /// It makes http requests to the person API endpoints.
    /// </summary>
    public class PersonClientRepository : IPersonRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl = $"http://localhost:3000/api/people";

        public PersonClientRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Invokes the http api to create a person on the server.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<Person> CreatePerson(EditPersonParameters parameters)
        {
            Person person = null;

            HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync(_serverUrl, parameters);
            if (responseMessage.IsSuccessStatusCode) 
            {
                person = await responseMessage.Content.ReadAsAsync<Person>();
            }

            return person;

        }

        /// <summary>
        /// Invokes the http api to delete a person from the server.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeletePerson(Guid id)
        {
            HttpResponseMessage responseMessage = await _httpClient.DeleteAsync(_serverUrl+$"/{ id}");
        }

        /// <summary>
        /// Invokes the http api to modify a person on the server.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<Person> EditPerson(Guid id, EditPersonParameters parameters)
        {
            Person person = null;


            /****************************************************************************
             * commented out section that I couldn't get working. Was trying to come up with a way to only pass the changed properties to the server. 
             * When passing parameters to the server and converting it to JSON it ends up losing  what was changed and sets both values to null.
             * 
             * **********************************************************************************************
                        if (parameters == null)
                            throw new ArgumentNullException(nameof(parameters));

                        var person = new Person
                        {
                            Id = Guid.NewGuid()
                        };



                        IEnumerable<string> changedProperties = parameters.GetChangedProperties();


                        EditPersonParameters onlyChanged = new EditPersonParameters();

                        parameters.Patch(person, onlyChanged);


                        foreach (string s in changedProperties) 
                        {

                            PropertyInfo onlyChangedObj = onlyChanged.GetType().GetProperty(s);
                            PropertyInfo unchangedObj = parameters.GetType().GetProperty(s);
                            //unchangedObj.GetValue
                            // make sure object has the property we are after
                            if (onlyChangedObj != null)
                            {
                                onlyChangedObj.SetValue(onlyChanged, unchangedObj);
                            }


                        }


                        string t = JsonSerializer.Serialize(onlyChanged).ToString();

                        */
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), _serverUrl + $"/{id}");
            request.Content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
            

            HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);
            if (responseMessage.IsSuccessStatusCode) 
            {
                person = await responseMessage.Content.ReadAsAsync<Person>();                    
            }

            return person;

        }

        /// <summary>
        /// Invokes the http api to return one or more people that match the query parameters.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Person>> GetPeople(QueryPersonParameters parameters)
        {
            IEnumerable<Person> iPerson = null;

            string url = _serverUrl;

            if (parameters != null) 
            {
                url = url +_CreateUrlFromQueryPersonParameters(parameters);
            }

            HttpResponseMessage responseMessage = await _httpClient.GetAsync(url);
            if (responseMessage.IsSuccessStatusCode)
            {
                iPerson = await responseMessage.Content.ReadAsAsync<IEnumerable<Person>>();
            }

            return iPerson;

        }

        /// <summary>
        /// Invokes the http api to return one person who matches the provided id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Person> GetPerson(Guid id)
        {
            Person person = null;

            HttpResponseMessage responseMessage = await _httpClient.GetAsync(_serverUrl+$"/{id}");
            if (responseMessage.IsSuccessStatusCode) {
                person = await responseMessage.Content.ReadAsAsync<Person>();
            }

            return person;

        }

        private static string _CreateUrlFromQueryPersonParameters(QueryPersonParameters parameters) 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("?");
            //bool isFirstInList = true;


            if (parameters.FirstName != null)
            {
                foreach (string f in parameters.FirstName)
                {
                    sb.Append("FirstName=").Append(f).Append("&");
                }
            }

            if (parameters.LastName != null)
            {
                foreach (string l in parameters.LastName)
                {
                    sb.Append("LastName=").Append(l).Append("&");
                }
            }

            if (parameters.Id != null)
            {
                foreach (Guid id in parameters.Id)
                {
                    sb.Append("Id=").Append(id.ToString()).Append("&");
                }
            }


            return sb.Remove(sb.Length - 1, 1).ToString(); // removeing the last "&" 
        
        }
        
    }
}