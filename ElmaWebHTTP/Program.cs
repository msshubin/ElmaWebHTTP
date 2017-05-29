using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace ElmaWebHTTP
{
    public static class StreamExtensions
    {
        public static byte[] ToByteArray(this Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }
    }

    class Program
    {
        public class Auth
        {
            public string AuthToken { get; set; }
            public string SessionToken { get; set; }
        }

        public static Stream DownloadFile(Auth auth, Guid fileguid)
        {
            Stream result = null;

            HttpWebRequest downloadfile = WebRequest.Create(String.Format("http://localhost:8000/API/REST/Files/Download?uid={0}", fileguid)) as HttpWebRequest;
            downloadfile.Method = "GET";
            downloadfile.Headers.Add("AuthToken", auth.AuthToken);
            downloadfile.Headers.Add("SessionToken", auth.SessionToken);
            downloadfile.Timeout = 10000;
            downloadfile.ContentType = "application / json; charset = utf - 8";

            var res = downloadfile.GetResponse() as HttpWebResponse;
              
            result = res.GetResponseStream();

            return result;
        }

        public static void UploadFile(Auth auth, FileStream file)
        {
            Guid fileguid;

            HttpWebRequest uploadfile = WebRequest.Create("http://localhost:8000/API/REST/Files/Upload") as HttpWebRequest;
            uploadfile.Method = "POST";
            uploadfile.Headers.Add("AuthToken", auth.AuthToken);
            uploadfile.Headers.Add("SessionToken", auth.SessionToken);
            //uploadfile.Timeout = 10000;
            //uploadfile.ContentType = "text/plain; charset = utf - 8";

            //uploadfile.ContentLength = file.ToByteArray().Length;


            Stream sendStream = uploadfile.GetRequestStream();
//            Console.WriteLine(sendStream.CanWrite);
            sendStream.SetLength(file.ToByteArray().Length);

            sendStream.Write(file.ToByteArray(), 0, file.ToByteArray().Length);
            sendStream.Close();

            // получение ответа
            System.Net.WebResponse result = uploadfile.GetResponse();
            var responseText = new StreamReader(result.GetResponseStream()).ReadToEnd();
            Console.WriteLine(responseText);

/*
            var res = uploadfile.GetResponse() as HttpWebResponse;
            var resStream = res.GetResponseStream();
            var sr = new StreamReader(resStream, Encoding.UTF8);
            var str = @sr.ReadToEnd();
            */



            //return null;
        }




        static void UploadFile3 (Auth auth)
        {
            string filePath = @"C:\Users\shubin\Desktop\обратка.txt";

            string url = "http://localhost:8000/API/REST/Files/Upload";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("AuthToken", auth.AuthToken);
            request.Headers.Add("SessionToken", auth.SessionToken);
            request.Headers.Add("FileName", "file.txt");
            request.Accept = "text/xml";
            request.Method = "POST";

            using (FileStream fileStream = File.OpenRead(filePath))
            using (Stream requestStream = request.GetRequestStream())
            {
                int bufferSize = 1024;
                byte[] buffer = new byte[bufferSize];
                int byteCount = 0;
                while ((byteCount = fileStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    requestStream.Write(buffer, 0, byteCount);
                }
            }

            string result;

            using (WebResponse response = request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
                
            }

            Console.WriteLine(result);
            Console.ReadKey();
        }

        public class MyGuid
        {
            public string Value { get; set; }
        }

        static void UploadFile2(Auth auth, string filename, string path)
        {
            var client = new RestClient("http://localhost:8000/API/REST/Files/Upload");

            var request = new RestRequest("resource/{id}", Method.POST);
            request.AddParameter("name", "value"); // adds to POST or URL querystring based on Method
            request.AddUrlSegment("id", "123"); // replaces matching token in request.Resource

            // easily add HTTP Headers
            request.AddHeader("AuthToken", auth.AuthToken);
            request.AddHeader("SessionToken", auth.SessionToken);
            request.AddHeader("FileName", "file.txt");

            // add files to upload (works with compatible verbs)
            request.AddFile(filename, path);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string
            Console.WriteLine(content.ToString());

            // or automatically deserialize result
            // return content type is sniffed but can be explicitly set via RestClient.AddHandler();
            var response2 = client.Execute<MyGuid>(request);
            var name = response2.Data.Value;
            
            // easy async support
            client.ExecuteAsync(request, response => {
                Console.WriteLine(response.Content);
            });

            // async with deserialization
            var asyncHandle = client.ExecuteAsync<Person>(request, response => {
                Console.WriteLine(response.Data.Name);
            });

            // abort the request on demand
            asyncHandle.Abort();
        }

        static void Main(string[] args)
        {

            //создаем токен приложения
            string ApplicationToken = "12D37077EF328E38F371E0159D28ADB6DE1C4C8C37100BADDB19CDF57BE39C682145B7BECB040842361F4F95BF77606DEF2426CE6F5B9FBF361EB82AC5AF9A6A"; // 3.10
            //string ApplicationToken = "57144414FA31EC96F282620B9F681822995CE9CDFC21B552845324BD017879FDD2F2CD621B4064DE66C7227F5518F2D669E64F6A6FCF5FD235425F239A41895D"; // 3.9

            //создаем веб запрос
            HttpWebRequest req = WebRequest.Create(String.Format("http://localhost:8000/API/REST/Authorization/LoginWith?username=admin")) as HttpWebRequest;
            req.Headers.Add("ApplicationToken", ApplicationToken);
            req.Method = "POST";
            req.Timeout = 10000;
            req.ContentType = "application/json; charset=utf-8";
 
            //данные для отправки. используется для передачи пароля. пароль нужно записать вместо пустой строки
            var sentData = Encoding.UTF8.GetBytes("");
            req.ContentLength = sentData.Length;
            Stream sendStream = req.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);
 
            //получение ответа
            var res = req.GetResponse() as HttpWebResponse;
            var resStream = res.GetResponseStream();
            var sr = new StreamReader(resStream, Encoding.UTF8);
            var str = @sr.ReadToEnd();

            /*
            //достаем необходимые данные при помощи библиотеки ЭЛМА в словарь
            var dict = new EleWise.ELMA.Serialization.JsonSerializer().Deserialize(str, typeof(Dictionary<string, string>)) as Dictionary<string, string>;

            var authToken = dict["AuthToken"];
            var sessionToken = dict["SessionToken"];
            Console.WriteLine("dict.AuthToken: " + dict["AuthToken"]);
            Console.WriteLine("dict.SessionToken: " + dict["SessionToken"]);
            Console.ReadKey();
            */
            //достаем необходимые данные при помощи библиотеки NewTon в объект
            Auth auth = JsonConvert.DeserializeObject<Auth>(str);
            
            Console.WriteLine("auth.AuthToken: " + auth.AuthToken);
            Console.WriteLine("auth.SessionToken: " + auth.SessionToken);
            //Console.ReadKey();

            /*
            Guid filename = new Guid("F4ECAF0B-83E2-4BED-831E-B68EABD86878");
            Stream file = DownloadFile(auth, filename);

            Console.WriteLine("******считываем весь файл********");

            using (StreamReader stream_reader = new StreamReader(file))
            {
                Console.WriteLine(stream_reader.ReadToEnd());
            }
            //Console.ReadKey();
            
            //Open the stream and read it back.
            */

            string path = @"C:\Users\shubin\Desktop\обратка.txt";
            FileStream fs = File.OpenRead(path);

            //UploadFile(auth, fs);
            UploadFile3(auth);

            /*
            using (FileStream fs = File.OpenRead(path))
            {
                fs.Re
                byte[] b = new byte[1024];
                UTF8Encoding temp = new UTF8Encoding(true);
                while (fs.Read(b, 0, b.Length) > 0)
                {
                    Console.WriteLine(temp.GetString(b));
                }
            }
            */

        }
    }
}
