using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Webkit;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Json;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
namespace Shopper
{
    [Activity(Label = "Shopper", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        public static AmazonDynamoDBConfig dbConfig = new AmazonDynamoDBConfig();
        public static AmazonDynamoDBClient dynDBClient;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            dbConfig.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamobdb/";
            dbConfig.AuthenticationRegion = "dynamodb.us-east-1.amazonaws.com";
            dbConfig.RegionEndpoint = RegionEndpoint.USEast1;

            AmazonDynamoDBClient dynDBClient = new AmazonDynamoDBClient("AKIAIMDIMZSEHYRAI6CQ", "6B2FRtd4JZiwq2iqiQJOmJPytboQ7EDOb08xovN3", dbConfig.RegionEndpoint);


            //dynDBClient.Config.ServiceURL= "https://console.aws.amazon.com/dynamodb/"; 
            dynDBClient.Config.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamodb/";
            dynDBClient.Config.RegionEndpoint = RegionEndpoint.USEast1;
           DynamoDBContext dynContext = new DynamoDBContext(dynDBClient);
            // LinearLayout llMain = FindViewById<LinearLayout>(Resource.Id.llMain);
            // WebView wvSearch = new WebView(this);
            // Uri uriSearch = new Uri("http://www.google.com/search?q=22630+stores");



            // Intent intentZip = new Intent(this,typeof(ZipCodeActivity));
            Intent intentShoppingDelivering = new Intent(this, typeof(GoShoppingActivity));
            StartActivity(intentShoppingDelivering);
            // Intent intentDeliveryPickup = new Intent(this, typeof(DeliveryPickupActivity));

         //  StartActivity(intentDeliveryPickup);


            // Get our button from the layout resource,
            // and attach an event to it
            //  List<string> lstResults = new List<string>();
            // Intent intentSearch = new Intent(Intent.ActionWebSearch);

            // Intent intentRes= intentSearch.PutExtra(SearchManager.Query, "22630 stores");


            // StartActivity(intentRes);
            //  TextView tvAddSearch = new TextView(this);
            // 
            // string strRes = intentSearch.GetStringExtra(SearchManager.Query);
            // tvAddSearch.Text = intentSearch.Get
            //  GridLayout grdWeb = new GridLayout(this);
            //  grdWeb.SetMinimumHeight(100);
            // grdWeb.AddView(wvSearch);
            //   llMain.AddView(grdWeb);
            // wvSearch.LoadUrl(uriSearch.ToString());
        }
    }
}

