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

[DynamoDBTable("ItemCategories")]
public class ItemCategories
{
    public string IcID { get; set; }
    public string CategoryName { get; set; }
}



namespace Shopper

{
    [Activity(Label = "List of Items")]
    public class ItemsActivity : Activity
    {
        public static AmazonDynamoDBConfig dbConfig = new AmazonDynamoDBConfig();
        public static AmazonDynamoDBClient dynDBClient;
        public static string strSelectedItem="";
        public static string strCustFName = "";
        public static string strCustLName = "";
        public static string strUserZip = "";
        public static string strSelectedStore= "";
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.ItemsLayout);
             strUserZip = this.Intent.GetStringExtra("UserZipCode");
             strSelectedStore = this.Intent.GetStringExtra("SelectedStore");
            if (this.Intent.GetStringExtra("CustomerFName") != "")
            {
                strCustFName = this.Intent.GetStringExtra("CustomerFName");
                strCustLName= this.Intent.GetStringExtra("CustomerLName");
            }
            GridLayout grdItems = FindViewById<GridLayout>(Resource.Id.grdItems);
            ImageButton imgAddItem = FindViewById<ImageButton>(Resource.Id.imgAddItem);
            TextView tvSelectedItemName = FindViewById<TextView>(Resource.Id.tvSelectedItemName);
            Button btnViewWebSearch = FindViewById<Button>(Resource.Id.btnViewWebSearch);
            Button btnToShoppingCart = FindViewById<Button>(Resource.Id.btnToShoppingCart);
            Button btnBack = FindViewById<Button>(Resource.Id.btnBack);
            dbConfig.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamobdb/";
            dbConfig.AuthenticationRegion = "dynamodb.us-east-1.amazonaws.com";
            dbConfig.RegionEndpoint = RegionEndpoint.USEast1;

            AmazonDynamoDBClient dynDBClient = new AmazonDynamoDBClient("AKIAIMDIMZSEHYRAI6CQ", "6B2FRtd4JZiwq2iqiQJOmJPytboQ7EDOb08xovN3", dbConfig.RegionEndpoint);


            //dynDBClient.Config.ServiceURL= "https://console.aws.amazon.com/dynamodb/"; 
            dynDBClient.Config.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamodb/";
            dynDBClient.Config.RegionEndpoint = RegionEndpoint.USEast1;
            DynamoDBContext dynContext = new DynamoDBContext(dynDBClient);

            AsyncSearch<ItemCategories> listItems = dynContext.FromScanAsync<ItemCategories>(new ScanOperationConfig()
            {

                ConsistentRead = true
            });

            List<ItemCategories> dataItems = await listItems.GetRemainingAsync();

            var theItems = from anItem in dataItems
                            where anItem.CategoryName != "none"
                            select anItem;


            this.listTheItems(dataItems, tvSelectedItemName, grdItems);
             strSelectedItem="";
            tvSelectedItemName.Text = "";

            btnViewWebSearch.Click += (sender, e) =>
            {
                Uri uriSearch = new Uri(string.Format("http://www.google.com/search?q={0}+{1}+product+categories+{2}", strUserZip, strSelectedStore,strSelectedItem));
                WebView webSearchItems = new WebView(this);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriSearch);
                request.Method = "GET";

                request.AllowWriteStreamBuffering = false;
                request.ContentType = "application/json";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
                string responseText = reader.ReadToEnd();
                string returnString = response.StatusCode.ToString();
                // editText1.Text = responseText;
                //   Toast.MakeText(this, responseText, ToastLength.Long).Show();
                webSearchItems.LoadUrl(uriSearch.ToString());
            };

            btnToShoppingCart.Click += (sender, e) =>
            {
                if (strSelectedStore.Trim() == "")
                {
                    var dlgBlankStore = new AlertDialog.Builder(this);
                    dlgBlankStore.SetMessage("Please select an item category!");
                    dlgBlankStore.SetNeutralButton("OK", delegate { });
                    dlgBlankStore.Show();
                }
                else
                { 
                    Intent intentCart = new Intent(this, typeof(ShoppingCartActivity));
                    intentCart.PutExtra("CustomerFName", strCustFName);
                    intentCart.PutExtra("CustomerLName", strCustLName);
                    intentCart.PutExtra("SelectedStore", strSelectedStore);
                    intentCart.PutExtra("ProductType", strSelectedItem);
                    intentCart.PutExtra("UserZipCode", strUserZip);
                    StartActivity(intentCart);
                }

            };

            btnBack.Click += (sender, e) =>
            {
                Intent intentStores=new Intent(this,typeof(StoresActivity));
                intentStores.PutExtra("UserZipCode", strUserZip);

                StartActivity(intentStores);
            };

            imgAddItem.Click += (sender, e) =>
            {
                var dlgAddNewItem = new AlertDialog.Builder(this);
                dlgAddNewItem.SetTitle("ADD A NEW ITEM");
                dlgAddNewItem.SetMessage("Please Enter a Product Type");
                
                EditText edtNewItemName = new EditText(this);
                //edtNewItemName.SetWidth(600);
               // grdNewStoreInfo.AddView(edtNewStoreName);
                dlgAddNewItem.SetView(edtNewItemName);
               
                dlgAddNewItem.SetPositiveButton("OK", delegate
                {
                    string strNewItemName = edtNewItemName.Text;
                  
                    var reqScanItems = new ScanRequest
                    {
                        TableName = "ItemCategories"
                    };
                    var respScanItems = dynDBClient.ScanAsync(reqScanItems);
                    int iItemCount = respScanItems.Result.Count;
                    Table tblTheItems = Table.LoadTable(dynDBClient, "ItemCategories");
                    Document docNewItem = new Document();
                    docNewItem["IcID"] = iItemCount.ToString();
                    docNewItem["CategoryName"] = strNewItemName;
                    
                    tblTheItems.PutItemAsync(docNewItem);
                    this.listTheItems(dataItems, tvSelectedItemName, grdItems);
               
                   
                });
                dlgAddNewItem.SetNegativeButton("CANCEL", delegate { });
                dlgAddNewItem.Show();
            };

            
        }

        public void listTheItems(List<ItemCategories> dataItems, TextView tvSelectedItemName, GridLayout grdItems)
        {

            dbConfig.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamobdb/";
            dbConfig.AuthenticationRegion = "dynamodb.us-east-1.amazonaws.com";
            dbConfig.RegionEndpoint = RegionEndpoint.USEast1;

            AmazonDynamoDBClient dynDBClient = new AmazonDynamoDBClient("AKIAIMDIMZSEHYRAI6CQ", "6B2FRtd4JZiwq2iqiQJOmJPytboQ7EDOb08xovN3", dbConfig.RegionEndpoint);


            //dynDBClient.Config.ServiceURL= "https://console.aws.amazon.com/dynamodb/"; 
            dynDBClient.Config.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamodb/";
            dynDBClient.Config.RegionEndpoint = RegionEndpoint.USEast1;
            DynamoDBContext dynContext = new DynamoDBContext(dynDBClient);

            AsyncSearch<ItemCategories> listItems = dynContext.FromScanAsync<ItemCategories>(new ScanOperationConfig()
            {

                ConsistentRead = true
            });

            var theItems = from anItem in dataItems
                           where anItem.CategoryName != "none"
                           select anItem;
            grdItems.RemoveAllViews();
            foreach (ItemCategories theItem in theItems)
            {
                TextView tvItem = new TextView(this) { Id = View.GenerateViewId() };
                tvItem.Text = theItem.CategoryName;
                strSelectedItem = tvItem.Text;
                tvItem.SetTextColor(Android.Graphics.Color.Black);
                tvItem.SetBackgroundColor(Android.Graphics.Color.White);
                tvItem.SetPadding(20, 5, 20, 5);
                tvItem.TextAlignment = TextAlignment.ViewStart;
                tvItem.SetWidth(1200);
                tvItem.SetBackgroundResource(Resource.Drawable.StoreName);
                tvItem.Click += (sender, e) =>
                {


                    strSelectedItem = tvItem.Text;
                    tvSelectedItemName.Text = strSelectedItem;
                    Uri uriSearch = new Uri(string.Format("http://www.google.com/search?q={0}+{1}+product+categories+{2}", strUserZip, strSelectedStore, strSelectedItem));
                    WebView webSearchItems = new WebView(this);
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriSearch);
                    request.Method = "GET";

                    request.AllowWriteStreamBuffering = false;
                    request.ContentType = "application/json";

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    var reader = new StreamReader(response.GetResponseStream());
                    string responseText = reader.ReadToEnd();
                    string returnString = response.StatusCode.ToString();
                    // editText1.Text = responseText;
                    //   Toast.MakeText(this, responseText, ToastLength.Long).Show();
                    webSearchItems.LoadUrl(uriSearch.ToString());

                };
                grdItems.AddView(tvItem);


            }
        }
    }
}