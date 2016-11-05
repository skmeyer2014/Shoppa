using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
[DynamoDBTable("CreditCard")]
public class CreditCardNotification
{
    public string ChargeID { get; set; }
    public string PurchaseDate { get; set; }
    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public string CustomerName { get; set; }
    public string Expiration { get; set; }
    public string Merchant { get; set; }
    public string ItemDescription { get; set; }
    public double Amount { get; set; }
    public string Verification { get; set; }
    public bool NewCharge { get; set; }
}
[DynamoDBTable("Delivery")]
public class DeliveryNotification
{
    public int DeliveryItemID { get; set; }
    public double Cost { get; set; }
    public string CreditCardNum { get; set; }
    public string CustomerName { get; set; }
    public string CustomerAddress { get; set; }
    public string CustomerCity { get; set; }
    public string CustomerState { get; set; }
    public string CustomerZip { get; set; }
    public string CustomerPhone { get; set; }
    public string ProductDescription { get; set; }
    public string DeliveryDate { get; set; }
    public string DeliveryTime { get; set; }
    public int NotifyID { get; set; }
}
namespace Shopper
{
    [Activity(Label = "DeliveryNotificationActivity")]
    public class DeliveryNotificationActivity : Activity
    {
        public static AmazonDynamoDBConfig dbConfig = new AmazonDynamoDBConfig();
        public static AmazonDynamoDBClient dynDBClient;
        protected override async void  OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.DeliveryNotification);
            dbConfig.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamobdb/";
            dbConfig.AuthenticationRegion = "dynamodb.us-east-1.amazonaws.com";
            dbConfig.RegionEndpoint = RegionEndpoint.USEast1;
            EditText edtDeliveryMessage = FindViewById<EditText>(Resource.Id.edtDeliveryMessage);
            AmazonDynamoDBClient dynDBClient = new AmazonDynamoDBClient("AKIAIMDIMZSEHYRAI6CQ", "6B2FRtd4JZiwq2iqiQJOmJPytboQ7EDOb08xovN3", dbConfig.RegionEndpoint);
            GridLayout grdDeliveryNotification = FindViewById<GridLayout>(Resource.Id.grdDeliveryNotification);

            //dynDBClient.Config.ServiceURL= "https://console.aws.amazon.com/dynamodb/"; 
            dynDBClient.Config.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamodb/";
            dynDBClient.Config.RegionEndpoint = RegionEndpoint.USEast1;
            DynamoDBContext dynContext = new DynamoDBContext(dynDBClient);
            Button btnHome = FindViewById<Button>(Resource.Id.btnHome);
            AsyncSearch<DeliveryNotification> listDeliveries = dynContext.FromScanAsync<DeliveryNotification>(new ScanOperationConfig()
            {

                ConsistentRead = true
            });


            List<DeliveryNotification> lstDataDeliveries = await listDeliveries.GetRemainingAsync();


            AsyncSearch<CreditCardNotification> listCCNotification = dynContext.FromScanAsync<CreditCardNotification>(new ScanOperationConfig()
            {

                ConsistentRead = true
            });


            List<CreditCardNotification> lstDataCCNotification = await listCCNotification.GetRemainingAsync();

            var theDeliveries = from aDelivery in lstDataDeliveries
                            where aDelivery.NotifyID==0
                            select aDelivery;
            int newDeliveryCount = 0;
            foreach (DeliveryNotification deliv in lstDataDeliveries)
            {
                if (deliv.DeliveryItemID == 0) break;
                TextView tvSubject = new TextView(this);
                tvSubject.Text = string.Concat(deliv.CustomerName, " ", deliv.ProductDescription, " ", deliv.DeliveryDate);
                tvSubject.SetBackgroundResource(Resource.Drawable.StoreName);
                tvSubject.SetTextColor(Android.Graphics.Color.Black);
                if (deliv.NotifyID == 0)
                {
                    ++newDeliveryCount;
                    tvSubject.SetTypeface(Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Bold);
                     
                }
                tvSubject.Click += (sender, e) =>
                {

                    CreditCardNotification ccPurchaseQuery = lstDataCCNotification.Find(x => x.CardNumber == deliv.CreditCardNum && x.ItemDescription == deliv.ProductDescription && x.Amount == deliv.Cost);
                    string strMerchant = ccPurchaseQuery.Merchant;
                    string strCCNum = ccPurchaseQuery.CardNumber;
                    StringBuilder sbMessage = new StringBuilder();
                    sbMessage.Append(deliv.CustomerName + " ordered " + deliv.ProductDescription + " from " + strMerchant);
                    sbMessage.AppendLine(" for " + deliv.Cost.ToString() + " to be delivered at ");
                    sbMessage.AppendLine(deliv.CustomerAddress + ", " + deliv.CustomerCity + ", " + deliv.CustomerState + ", " + deliv.CustomerZip);
                    sbMessage.AppendLine(" on " + deliv.DeliveryDate + " time: " + deliv.DeliveryTime + ", CC#" + strCCNum);
                    edtDeliveryMessage.Text = sbMessage.ToString();
                   // edtDeliveryMessage.Text = string.Format("{0} ordered {1} from {2} for {3} to be delivered at {4}, {5}, {6}, {7} on {8}, Time: {9) CC# {10}", deliv.CustomerName, deliv.ProductDescription,strMerchant,deliv.Cost, deliv.CustomerAddress, deliv.CustomerCity, deliv.CustomerState, deliv.CustomerZip,deliv.DeliveryDate, deliv.DeliveryTime,strCCNum);

                };
                grdDeliveryNotification.AddView(tvSubject);
                
            }
            if(newDeliveryCount>0)
            {
                var dlgNewDeliveryNote = new AlertDialog.Builder(this);
                dlgNewDeliveryNote.SetMessage("You have new orders to deliver.");
                dlgNewDeliveryNote.SetNeutralButton("OK",delegate { });
                dlgNewDeliveryNote.Show();
            }
            btnHome.Click += (sender, e) =>
            {
                var intentHomeScreen = new Intent(this, typeof(GoShoppingActivity));
                StartActivity(intentHomeScreen);
            };
        }
    }
}