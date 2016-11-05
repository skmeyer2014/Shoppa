using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;


[DynamoDBTable("RetailStores")]
public class RetailStores
{
    public string StoreID { get; set; }
    public string StoreName { get; set; }
    public string StoreURL { get; set; }
   
}

namespace Shopper
{
    [Activity(Label = "List of Stores")]
    public class StoresActivity : Activity
    {
        public static AmazonDynamoDBConfig dbConfig = new AmazonDynamoDBConfig();
        public static AmazonDynamoDBClient dynDBClient;
        public static string strSelectedStore = "";
        public string strUserZip = "";
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.StoresLayout);
            strUserZip = Intent.GetStringExtra("UserZipCode");
            GridLayout grdStores = FindViewById<GridLayout>(Resource.Id.grdStores);
            ImageButton imgAddStore = FindViewById<ImageButton>(Resource.Id.imgAddStore);
            Button btnToItems= FindViewById<Button>(Resource.Id.btnToItems);
            Button btnViewWebSite = FindViewById<Button>(Resource.Id.btnViewWebSite);
            TextView tvSelectedStoreName = FindViewById<TextView>(Resource.Id.tvSelectedStoreName);
            dbConfig.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamobdb/";
            dbConfig.AuthenticationRegion = "dynamodb.us-east-1.amazonaws.com";
            dbConfig.RegionEndpoint = RegionEndpoint.USEast1;

            AmazonDynamoDBClient dynDBClient = new AmazonDynamoDBClient("AKIAIMDIMZSEHYRAI6CQ", "6B2FRtd4JZiwq2iqiQJOmJPytboQ7EDOb08xovN3", dbConfig.RegionEndpoint);


            //dynDBClient.Config.ServiceURL= "https://console.aws.amazon.com/dynamodb/"; 
            dynDBClient.Config.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamodb/";
            dynDBClient.Config.RegionEndpoint = RegionEndpoint.USEast1;
            DynamoDBContext dynContext = new DynamoDBContext(dynDBClient);
            
            AsyncSearch<RetailStores> listStores = dynContext.FromScanAsync<RetailStores>(new ScanOperationConfig()
            {

                ConsistentRead = true
            });

            List<RetailStores> dataStores = await listStores.GetRemainingAsync();

            this.listTheStores(dataStores, tvSelectedStoreName,grdStores);
          


           strSelectedStore="";

            btnViewWebSite.Click += (sender, e) =>
            {
                Uri uriSearch = new Uri(string.Format("http://www.google.com/search?q=list+stores+near+{0}+{1} ", strUserZip,strSelectedStore));
               
                WebView webStores = new WebView(this);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriSearch);
                request.Method = "GET";

                request.AllowWriteStreamBuffering = false;
                request.ContentType = "application/json";
                

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
               
                
              
                string responseText = reader.ReadToEnd();

                string returnString = response.StatusCode.ToString();
                // editText1.Text = responseText;
                //  Toast.MakeText(this, responseText, ToastLength.Long).Show();
                webStores.LoadUrl(uriSearch.ToString());
               
            };

            btnToItems.Click += (sender, e) =>
            {
                Intent intentItems = new Intent(this, typeof(ItemsActivity));
                intentItems.PutExtra("UserZipCode", strUserZip);
                intentItems.PutExtra("SelectedStore", strSelectedStore);
                var dlgToItemsScreen = new AlertDialog.Builder(this);
                if (strSelectedStore.Trim()== "")
                {
                    dlgToItemsScreen.SetMessage("PLEASE SELECT A STORE!");
                    dlgToItemsScreen.SetNeutralButton("OK", delegate { });

                }
                else
                {
                    dlgToItemsScreen.SetTitle(string.Format("{0} in or near {1}", strSelectedStore, strUserZip));
                    dlgToItemsScreen.SetMessage(string.Format("This will take you to the Items screen based on your search\nfor {0} in/near your area, is this OK?", strSelectedStore, strUserZip));
                    dlgToItemsScreen.SetPositiveButton("OK", delegate
                    {
                        StartActivity(intentItems);
                    });
                    dlgToItemsScreen.SetNegativeButton("CANCEL", delegate { });

                }
                dlgToItemsScreen.Show();

            };
           imgAddStore.Click += (sender, e) =>
            {
                var dlgAddNewStore = new AlertDialog.Builder(this);
                dlgAddNewStore.SetTitle("ADD A NEW STORE");
                dlgAddNewStore.SetMessage("Please Enter a Name for a Store");
                GridLayout grdNewStoreInfo = new GridLayout(this);
                grdNewStoreInfo.RowCount = 4;
                grdNewStoreInfo.ColumnCount = 1;
                EditText edtNewStoreName = new EditText(this);
                edtNewStoreName.SetWidth(600);
                grdNewStoreInfo.AddView(edtNewStoreName);
                //dlgAddNewStore.SetView(edtNewStoreName);
                TextView tvStoreURL = new TextView(this);
                tvStoreURL.SetTextColor(Android.Graphics.Color.White);
                tvStoreURL.Text = "Store URL (web address if known, otherwise we'll search):";
                grdNewStoreInfo.AddView(tvStoreURL);
               // dlgAddNewStore.SetView(tvStoreURL);
                EditText edtStoreURL = new EditText(this);
                edtStoreURL.SetWidth(600);
                grdNewStoreInfo.AddView(edtStoreURL);
                // dlgAddNewStore.SetView(edtStoreURL);
                dlgAddNewStore.SetView(grdNewStoreInfo);
                dlgAddNewStore.SetPositiveButton("OK", delegate
                 {
                     string strNewStoreName = edtNewStoreName.Text;
                     string strNewStoreURL = edtStoreURL.Text;
                     var reqScanStores = new ScanRequest
                     {
                         TableName = "RetailStores"
                     };
                     var respScanStores = dynDBClient.ScanAsync(reqScanStores);
                     int iStoreCount = respScanStores.Result.Count;
                     Table tblTheStores = Table.LoadTable(dynDBClient, "RetailStores");
                     Document docNewStore = new Document();
                     docNewStore["StoreID"] = iStoreCount.ToString();
                     docNewStore["StoreName"] = strNewStoreName;
                    if (strNewStoreURL != "") docNewStore["StoreURL"] = string.Concat("www.", strNewStoreName, ".com");
                     tblTheStores.PutItemAsync(docNewStore);
                     
                     //this.listTheStores(dataStores,tvSelectedStoreName,grdStores);
                 });
                dlgAddNewStore.SetNegativeButton("CANCEL", delegate { });
                dlgAddNewStore.Show();
                
            };

           
        }
        public async void listTheStores(List<RetailStores> dataStores, TextView tvSelectedStoreName, GridLayout grdStores)
        {


            dbConfig.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamobdb/";
            dbConfig.AuthenticationRegion = "dynamodb.us-east-1.amazonaws.com";
            dbConfig.RegionEndpoint = RegionEndpoint.USEast1;

            AmazonDynamoDBClient dynDBClient = new AmazonDynamoDBClient("AKIAIMDIMZSEHYRAI6CQ", "6B2FRtd4JZiwq2iqiQJOmJPytboQ7EDOb08xovN3", dbConfig.RegionEndpoint);


            //dynDBClient.Config.ServiceURL= "https://console.aws.amazon.com/dynamodb/"; 
            dynDBClient.Config.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamodb/";
            dynDBClient.Config.RegionEndpoint = RegionEndpoint.USEast1;
            DynamoDBContext dynContext = new DynamoDBContext(dynDBClient);

            AsyncSearch<RetailStores> listStores = dynContext.FromScanAsync<RetailStores>(new ScanOperationConfig()
            {

                ConsistentRead = true
            });

             dataStores = await listStores.GetRemainingAsync();

            var theStores = from store in dataStores
                            where store.StoreName != "none"
                            select store;
            grdStores.RemoveAllViews();
            foreach (RetailStores store in theStores)
            {
                TextView tvStore = new TextView(this) { Id = View.GenerateViewId() };
                 tvStore.Text = store.StoreName;
                strSelectedStore = tvStore.Text;
                tvStore.SetTextColor(Android.Graphics.Color.Black);
                tvStore.SetBackgroundColor(Android.Graphics.Color.White);
                tvStore.SetPadding(20, 5, 20, 5);
                tvStore.TextAlignment = TextAlignment.ViewStart;
                tvStore.SetWidth(1200);
                tvStore.SetBackgroundResource(Resource.Drawable.StoreName);
                tvStore.Click += (sender, e) => { 
                
                   
                    strSelectedStore = tvStore.Text;
                    tvSelectedStoreName.Text = strSelectedStore;
                  
                   Uri uriSearch = new Uri(string.Format("http://www.google.com/search?q=list+stores+near+{0}+{1} ", strUserZip, strSelectedStore));

                   WebView webStores = new WebView(this);
                   HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriSearch);
                   request.Method = "GET";

                   request.AllowWriteStreamBuffering = false;
                   request.ContentType = "application/json";


                   HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                   var reader = new StreamReader(response.GetResponseStream());



                   string responseText = reader.ReadToEnd();

                   string returnString = response.StatusCode.ToString();
                   
                    // editText1.Text = responseText;
                    //  Toast.MakeText(this, responseText, ToastLength.Long).Show();
                     webStores.LoadUrl(uriSearch.ToString());

                };
                grdStores.AddView(tvStore);


            }
       }
    }
}