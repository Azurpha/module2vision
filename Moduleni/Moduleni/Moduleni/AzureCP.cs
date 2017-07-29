using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Using a Modified MSA Model for moduleII. Practically the same due to structure. 
//Also because I have no idea how I could write this much different due to C# being an unfamiliar language.
namespace Moduleni
{
    public class AzureCP
    {
        private static AzureCP instance;
        private MobileServiceClient client;
        private IMobileServiceTable<PhoneModel> aTable;
        private AzureCP()
        {
            this.client = new MobileServiceClient("https://moduleii.azurewebsites.net");
            this.aTable = this.client.GetTable<PhoneModel>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureCP AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureCP();
                }

                return instance;
            }
        }
        public async Task<List<PhoneModel>> Gettinginfo()
        {
            return await this.aTable.ToListAsync();
        }
        public async Task PostInfo(PhoneModel phoneModel)
        {
            try
            {
                await this.aTable.UpdateAsync(phoneModel);
                
            }
            catch (Exception e)
            {
                await this.aTable.InsertAsync(phoneModel);

            }
        }
        public async Task DeleteInfo(PhoneModel phoneModel)
        {
            await this.aTable.DeleteAsync(phoneModel);
        }
    }
}
