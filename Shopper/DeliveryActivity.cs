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
public class DeliveryCreditCard
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
 public class Delivery
{
    public int DeliveryItemID { get; set; }
    public double Cost { get; set; }
    public string CreditCardNum { get; set; }
    public string CustomerName { get; set; }
    public string CustomerAddress { get; set; }
    public string CustomerCity { get; set; }
    public string CustomerState { get; set; }
    public string CustomerZip{ get; set; }
    public string CustomerPhone{ get; set; }
    public string ProductDescription { get; set; }
    public string DeliveryDate { get; set; }
    public string DeliveryTime { get; set; }
    public int NotifyID { get; set; }
}
namespace Shopper
{

   

    [Activity(Label = "DeliveryActivity")]
    public class DeliveryActivity : Activity
    {
        public static AmazonDynamoDBConfig dbConfig = new AmazonDynamoDBConfig();
        public static AmazonDynamoDBClient dynDBClient;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
                
            // Create your application here
            SetContentView(Resource.Layout.Deliverylayout);
            EditText edtCustName = FindViewById<EditText>(Resource.Id.edtCustName);
            EditText edtCustAddress = FindViewById<EditText>(Resource.Id.edtCustAddress);
            EditText edtCustPhone = FindViewById<EditText>(Resource.Id.edtCustPhone);
            EditText edtCustZip = FindViewById<EditText>(Resource.Id.edtCustZip);
            EditText edtCustCity = FindViewById<EditText>(Resource.Id.edtCustCity);
            EditText edtCustState = FindViewById<EditText>(Resource.Id.edtCustState);
            Button btnDeliver = FindViewById<Button>(Resource.Id.btnDeliver);
            Button btnHome = FindViewById<Button>(Resource.Id.btnHome);
            DatePicker datePicker1 = FindViewById<DatePicker>(Resource.Id.datePicker1);
            TimePicker timePicker1 = FindViewById<TimePicker>(Resource.Id.timePicker1);
            string strCustName = edtCustName.Text;
            string strCustAddress = edtCustAddress.Text;
            string strCustPhone = edtCustPhone.Text;
            string strCustCity = edtCustCity.Text;
            string strCustState= edtCustName.Text;
            string strCustZip = edtCustZip.Text;
            string strDeliveryDate = datePicker1.DateTime.ToString("MM/dd/yyyy");
            string strDeliveryTime = string.Format("{0}:{1}",timePicker1.CurrentHour,timePicker1.CurrentMinute);
            //Toast.MakeText(this, strDeliveryDate + " " + strDeliveryTime, ToastLength.Long).Show(); 
            dbConfig.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamobdb/";
            dbConfig.AuthenticationRegion = "dynamodb.us-east-1.amazonaws.com";
            dbConfig.RegionEndpoint = RegionEndpoint.USEast1;

            AmazonDynamoDBClient dynDBClient = new AmazonDynamoDBClient("AKIAIMDIMZSEHYRAI6CQ", "6B2FRtd4JZiwq2iqiQJOmJPytboQ7EDOb08xovN3", dbConfig.RegionEndpoint);


            //dynDBClient.Config.ServiceURL= "https://console.aws.amazon.com/dynamodb/"; 
            dynDBClient.Config.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamodb/";
            dynDBClient.Config.RegionEndpoint = RegionEndpoint.USEast1;
            DynamoDBContext dynContext = new DynamoDBContext(dynDBClient);

            AsyncSearch<DeliveryCreditCard> listCreditCardItems = dynContext.FromScanAsync<DeliveryCreditCard>(new ScanOperationConfig()
            {

                ConsistentRead = true
            });


            List<DeliveryCreditCard> lstDataNewCharges = await listCreditCardItems.GetRemainingAsync();

            AsyncSearch<Delivery> listPastDeliveries = dynContext.FromScanAsync<Delivery>(new ScanOperationConfig()
            {

                ConsistentRead = true
            });


            List<Delivery> lstDataPastDeliveries = await listPastDeliveries.GetRemainingAsync();
           
            int iPastDelivCount = lstDataPastDeliveries.Count;
            string strNotifyID = "0";
            var currentCharges = from aCharge in lstDataNewCharges
                                 where aCharge.NewCharge == true
                                 select aCharge;



            btnHome.Click += (sender, e) =>
            {
                var intentHomeScreen = new Intent(this, typeof(GoShoppingActivity));
                StartActivity(intentHomeScreen);
            };
            btnDeliver.Click += (sender, e) =>
            {
                var dlgAddressOK = new AlertDialog.Builder(this);
                dlgAddressOK.SetMessage("Are you sure you want to deliver to this address?");
                dlgAddressOK.SetPositiveButton("OK", delegate
                {

                    strCustName = edtCustName.Text;
                    strCustAddress = edtCustAddress.Text;
                     strCustPhone = edtCustPhone.Text;
                     strCustCity = edtCustCity.Text;
                     strCustState = edtCustState.Text;
                     strCustZip = edtCustZip.Text;
                     strDeliveryDate = datePicker1.DateTime.ToString("MM/dd/yyyy");
                    strDeliveryTime = string.Format("{0}:{1}", timePicker1.CurrentHour, timePicker1.CurrentMinute);
                    foreach (DeliveryCreditCard aCharge in currentCharges)
                    {
                        Table tblCreditCard = Table.LoadTable(dynDBClient, "CreditCard");
                        Table tblDelivery = Table.LoadTable(dynDBClient, "Delivery");
                        Document docDelivery = new Document();
                        Document docCreditCard = new Document();
                       
                        docDelivery["DeliveryItemID"] = (++iPastDelivCount).ToString();
                        docDelivery["CustomerName"] = strCustName;
                        docDelivery["CustomerAddress"] = strCustAddress;
                        docDelivery["CustomerCity"] = strCustCity;
                        docDelivery["CustomerState"] = strCustState;
                        docDelivery["CustomerPhone"] = strCustPhone;
                        docDelivery["NotifyID"] = strNotifyID;
                        docDelivery["CustomerZip"] = strCustZip;
                        docDelivery["CreditCardNum"] = aCharge.CardNumber;
                        docDelivery["ProductDescription"] = aCharge.ItemDescription;
                        docDelivery["DeliveryDate"] = strDeliveryDate;
                        docDelivery["DeliveryTime"] = strDeliveryTime;
                        docDelivery["Cost"] = aCharge.Amount;
                        tblDelivery.PutItemAsync(docDelivery);
                        docCreditCard["ChargeID"] = aCharge.ChargeID;
                        docCreditCard["NewCharge"] = 0;
                        tblCreditCard.UpdateItemAsync(docCreditCard);
                    }
                    Toast.MakeText(this, "Your order placed successfully, the delivery person will receive notification.", ToastLength.Long);


                });
                dlgAddressOK.SetNegativeButton("NO", delegate { });
                dlgAddressOK.Show();
            };
           
        }
    }
}