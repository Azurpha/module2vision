using Newtonsoft.Json;
using Plugin.Geolocator;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
//Using a base MSA Model for moduleII. Ofcourse, customized to do a slightly different thing.
//known bug: pressing "Pic Info" first before Take Photo will breaks loop, data will be shown incorrectly.
namespace Moduleni
{
    public partial class MainPage : ContentPage
    {
        private int identifier = 1;
        public MainPage()
        {
            InitializeComponent();
        }
        private async void loadCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                Directory = "Sample",
                Name = $"{DateTime.UtcNow}.jpg"
            });

            if (file == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });
            await MakePredictionRequest(file);
        }
        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }
        async void Listalldatainsql(object sender, System.EventArgs e)
        {
            List<PhoneModel> allinfo = await AzureCP.AzureManagerInstance.Gettinginfo();

            sqList.ItemsSource = allinfo;
        }
        public async void Handle_ClickedAsync(object sender, System.EventArgs e)
        {
            try
            {
                List<PhoneModel> model = await AzureCP.AzureManagerInstance.Gettinginfo();
                PhoneModel currentModel = new PhoneModel();
                if (model.ElementAt(identifier-1).ID == (identifier).ToString())
                    currentModel = model.ElementAt(identifier - 1);
                bool isitaphone = currentModel.IsPhone;
                String brand = currentModel.Brand;
                String phoneProb = currentModel.PProbability.ToString();
                String brandProb = currentModel.BProbability.ToString();
                if (isitaphone == true)
                {
                    await DisplayAlert("Info", "In the photo, there is a phone. \nProbability of this being true: " + phoneProb +
      "\nBrand: " + brand + " \nProbability of being correct: " + brandProb, "Close");
                }
                else
                {
                    await DisplayAlert("Info", brand + "\nProbability of this being true: " + phoneProb, "Close");
                }
            }
            catch (Exception se)
            {
                try
                {
                    List<PhoneModel> model = await AzureCP.AzureManagerInstance.Gettinginfo();
                    PhoneModel currentModel = new PhoneModel();
                    currentModel = model.ElementAt(0);
                    bool isitaphone = currentModel.IsPhone;
                    String brand = currentModel.Brand;
                    String phoneProb = currentModel.PProbability.ToString();
                    String brandProb = currentModel.BProbability.ToString();
                    if (isitaphone == true)
                    {
                        await DisplayAlert("Info", "In the photo, there is a phone. \nProbability of this being true: " + phoneProb +
          "\nBrand: " + brand + " \nProbability of being correct: " + brandProb, "Close");
                    }
                    else
                    {
                        await DisplayAlert("Info", brand + "\nProbability of this being true: " + phoneProb, "Close");
                    }
                }
                catch (Exception xes)
                {

                }
            }


        }

        //Note: my customvision AI isn't very well trained, its kinda of dumb but hey its only less than a week old give it a chance ;)
        async Task MakePredictionRequest(MediaFile file)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "472a93c611f141a999252019aefe238c");

            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/398372fe-18d3-4eff-b7be-68b8d84eebc8/image?iterationId=cb1dce3a-8a92-4098-9bfa-9ed6eda85cd4";

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);


                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    AnalysisingModel responseModel = JsonConvert.DeserializeObject<AnalysisingModel>(responseString);

                    List<Prediction> Predmodel = responseModel.Predictions;
                   
                    bool isitaphone = false;
                    String theBrand = "Other";
                    double bp = 0 , pp = 0, sp = 0, ip =0;
                    for (int i = 0; i < Predmodel.Count; i++)
                    {
                        if ("Phone" == Predmodel.ElementAt(i).Tag)
                        {
                            pp = Predmodel.ElementAt(i).Probability;
                            if (Predmodel.ElementAt(i).Probability >= 0.5)
                            {
                                isitaphone = true;
                            }
                        }
                        else if ("iPhone" == Predmodel.ElementAt(i).Tag)
                            ip = Predmodel.ElementAt(i).Probability;
                        else if ("samsung" == Predmodel.ElementAt(i).Tag)
                            sp = Predmodel.ElementAt(i).Probability;
                    }
                    if (ip == sp)
                        theBrand = "Unknown";
                    else if (ip > sp && (ip > 0.5 || sp > 0.5))
                    {
                        theBrand = "iPhone";
                        bp = ip;
                    }
                    else if (ip < sp && (ip > 0.5 || sp > 0.5))
                    {
                        theBrand = "Samsung";
                        bp = sp;
                    }
                    if (isitaphone == false)
                    {
                        bp = 0;
                        theBrand = "Not a Phone";
                    }
                    PhoneModel Datasend = new PhoneModel()
                    {
                        ID = identifier.ToString(),
                        IsPhone = isitaphone,
                        PProbability = pp,
                        Brand = theBrand,
                        BProbability = bp
                    };
                    identifier++;
                    await AzureCP.AzureManagerInstance.PostInfo(Datasend);
                }

            }
            file.Dispose();
        }
    }
}
