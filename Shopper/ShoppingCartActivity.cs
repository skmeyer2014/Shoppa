using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
public class CreditCard
{
    public string ChargeID { get; set; }
    public string PurchaseDate { get; set; }
    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public string CustomerName { get; set; }
    public string Expiration{ get; set; }
    public string Merchant { get; set; }
    public string ItemDescription { get; set; }
    public double Amount{ get; set; }
    public string Verification{ get; set; }
    public bool NewCharge { get; set; }
}

[DynamoDBTable("ShoppingCart")]
public class ShoppingCart
{
    public string OrderID { get; set; }
    public string OrderDate { get; set; }
    public string CustomerFname { get; set; }
    public string CustomerLname { get; set; }
    public string StoreName { get; set; }
    public string ProductDescription { get; set; }
    public double UnitPrice { get; set; }
    public int Quantity { get; set; }
    public double TotalCost{ get; set; }
    public bool CheckedOut { get; set; }
}

namespace Shopper
{
    [Activity(Label = "Shopping Cart")]
    public class ShoppingCartActivity : Activity
    {
        public static AmazonDynamoDBConfig dbConfig = new AmazonDynamoDBConfig();
        public static AmazonDynamoDBClient dynDBClient;
  
        public static string strCustFName = "";
        public static string strCustLName = "";
        public static string strStoreName = "";
        public static string strZipCode = "";
        public static string strSelectItem = "";
        public static double dblPrice = 0;
        public static string strOrderDate = "";
        public static int iCustNo = 0;
        public static string strCustNo = "";
        public static double dblTotalPurchase = 0.00;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.ShoppingCartLayout);

            dbConfig.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamobdb/";
            dbConfig.AuthenticationRegion = "dynamodb.us-east-1.amazonaws.com";
            dbConfig.RegionEndpoint = RegionEndpoint.USEast1;

            AmazonDynamoDBClient dynDBClient = new AmazonDynamoDBClient("AKIAIMDIMZSEHYRAI6CQ", "6B2FRtd4JZiwq2iqiQJOmJPytboQ7EDOb08xovN3", dbConfig.RegionEndpoint);


            //dynDBClient.Config.ServiceURL= "https://console.aws.amazon.com/dynamodb/"; 
            dynDBClient.Config.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamodb/";
            dynDBClient.Config.RegionEndpoint = RegionEndpoint.USEast1;
            DynamoDBContext dynContext = new DynamoDBContext(dynDBClient);

            AsyncSearch<ShoppingCart> listDataCartItems = dynContext.FromScanAsync<ShoppingCart>(new ScanOperationConfig()
            {

                ConsistentRead = true
            });


            List<ShoppingCart> dataCartItems = await listDataCartItems.GetRemainingAsync();
            Button btnCheckout = FindViewById<Button>(Resource.Id.btnCheckout);
            Button btnBack = FindViewById<Button>(Resource.Id.btnBack);
            Button btnEnterOrder= FindViewById<Button>(Resource.Id.btnEnterOrder);
            GridLayout grdOrderEntry = FindViewById<GridLayout>(Resource.Id.grdOrderEntry);
            TextView tvCustName = FindViewById<TextView>(Resource.Id.tvCustName);
            EditText edtItemDescription = FindViewById<EditText>(Resource.Id.edtItemDescription);
            EditText edtPrice = FindViewById<EditText>(Resource.Id.edtPrice);
            EditText edtQuant = FindViewById<EditText>(Resource.Id.edtQuant);
            DateTime dtOrderDate = DateTime.Today;
            strOrderDate = dtOrderDate.ToString("MM/dd/yyyy");
            strCustFName = this.Intent.GetStringExtra("CustomerFName");
            strCustLName = this.Intent.GetStringExtra("CustomerLName");
            strStoreName = this.Intent.GetStringExtra("SelectedStore");
            strZipCode = this.Intent.GetStringExtra("UserZipCode");
            strSelectItem = this.Intent.GetStringExtra("ProductType");
            var dlgCustomerName = new AlertDialog.Builder(this);
            GridLayout grdShoppingCart = FindViewById<GridLayout>(Resource.Id.grdShoppingCart);
            GridLayout grdCustName = new GridLayout(this);
            grdCustName.RowCount = 2;
            grdCustName.ColumnCount = 2;
            TextView tvFName = new TextView(this);
            tvFName.Text = "First Name:";
            grdCustName.AddView(tvFName);
            EditText edtFName = new EditText(this);
            grdCustName.AddView(edtFName);
            TextView tvLName = new TextView(this);
            tvLName.Text = "Last Name";
            grdCustName.AddView(tvLName);
            EditText edtLName = new EditText(this);
            grdCustName.AddView(edtLName);
            dlgCustomerName.SetTitle("PLEASE ENTER YOUR NAME");
            dlgCustomerName.SetView(grdCustName);
            dlgCustomerName.SetPositiveButton("OK", delegate {
                strCustFName = edtFName.Text;
                strCustLName = edtLName.Text;
                tvCustName.Text = string.Concat(strCustFName, " ", strCustLName);
                listCartItems(dataCartItems, grdShoppingCart);
            });
            dlgCustomerName.SetNegativeButton("CANCEL", delegate { });
            dlgCustomerName.Show();



           
            var theCart = from aCartItem in dataCartItems
                          where aCartItem.CustomerFname == strCustFName && aCartItem.CustomerLname == strCustLName && aCartItem.CheckedOut==false
                          select aCartItem;
            
            TextView tvCustStore = FindViewById<TextView>(Resource.Id.tvCustStore);
            tvCustStore.Text = strStoreName;

            listCartItems(dataCartItems, grdShoppingCart);

            btnCheckout.Click += (sender, e) =>
            {
                var dlgCheckout = new AlertDialog.Builder(this);
                dlgCheckout.SetMessage("Are you sure you want to check out your order?");
                
                GridLayout grdPurchase = new GridLayout(this);
                grdPurchase.ColumnCount = 1;
                grdPurchase.RowCount = 10;
                RadioGroup rgCreditCards = new RadioGroup(this);
                RadioButton rbVisa = new RadioButton(this);
                rbVisa.Text = "VISA";
                rgCreditCards.AddView(rbVisa);
                RadioButton rbMasterCard = new RadioButton(this);
                rbMasterCard.Text = "MASTER CARD";
                rgCreditCards.AddView(rbMasterCard);
                RadioButton rbDiscover = new RadioButton(this);
                rbDiscover.Text = "DISCOVER";
                rgCreditCards.AddView(rbDiscover);
                RadioButton rbAmEx = new RadioButton(this);
                rbAmEx.Text = "AMERICAN EXPRESS";
                rgCreditCards.AddView(rbAmEx);
                grdPurchase.AddView(rgCreditCards);
                TextView tvCCPrompt = new TextView(this);
                tvCCPrompt.Text = "YOUR CREDIT CARD NUMBER:";
                 grdPurchase.AddView(tvCCPrompt);
                EditText edtCCNum = new EditText(this);
                grdPurchase.AddView(edtCCNum);
                TextView tvExpDate = new TextView(this);
                tvExpDate.Text = "EXPIRATION DATE (mmyy):";
                EditText edtExpDate = new EditText(this);
                grdPurchase.AddView(tvExpDate);
                grdPurchase.AddView(edtExpDate);
                
                TextView tvVerifyCode = new TextView(this);
                tvVerifyCode.Text = "YOUR THREE-DIGIT VERIFICATION CODE:";
                EditText edtVerifyCode = new EditText(this);
                grdPurchase.AddView(tvVerifyCode);
                grdPurchase.AddView(edtVerifyCode);
                dlgCheckout.SetView(grdPurchase);
                
               
                string strCCName ="" ;
                bool boolCCSelected = false;
                //Toast.MakeText(this, grdPurchase.ChildCount.ToString(), ToastLength.Long).Show();
                dlgCheckout.SetPositiveButton("OK", async delegate
                {
                   // EditText edtTheNumber = (EditText)grdPurchase.GetChildAt(2);
                    string strCCNum = edtCCNum.Text;
                    string strExpDate = edtExpDate.Text;
                    string strVerifyCode = edtVerifyCode.Text;
                   
                    string strExpDatePattern = @"^(0[1-9]|1[012])\d{2}$";
                    for (int iCCN = 0; iCCN < rgCreditCards.ChildCount; iCCN++)
                    {
                        RadioButton rbCC = (RadioButton)rgCreditCards.GetChildAt(iCCN);
                        if (rbCC.Checked)
                        {
                            boolCCSelected = true;
                            strCCName = rbCC.Text;
                            break;
                        }
                    }
                    if (!boolCCSelected)
                    {
                        Toast.MakeText(this, "PLEASE SELECT A CREDIT CARD", ToastLength.Long).Show();
                    }
                    else if (strCCNum.Trim() == "")
                    {
                        Toast.MakeText(this, "PLEASE ENTER YOUR CREDIT INFORMATION", ToastLength.Long).Show();
                    }
                    else if (Regex.IsMatch(strExpDate, strExpDatePattern) == false)
                    {
                        Toast.MakeText(this, "PLEASE ENTER YOUR EXPIRATION DATE IN FORM mmyy", ToastLength.Long).Show();
                    }
                    else
                    {
                        
                        AsyncSearch<CreditCard> listPurchases = dynContext.FromScanAsync<CreditCard>(new ScanOperationConfig()
                        {

                            ConsistentRead = true
                        });
                        List<CreditCard> listDataPurchases = await listPurchases.GetRemainingAsync();

                        int iCountAllPurchases = listDataPurchases.Count;
                       listDataCartItems = dynContext.FromScanAsync<ShoppingCart>(new ScanOperationConfig()
                        {

                            ConsistentRead = true
                        });


                         dataCartItems = await listDataCartItems.GetRemainingAsync();


                        theCart = from aCartItem in dataCartItems
                                  where aCartItem.CustomerFname == strCustFName && aCartItem.CustomerLname == strCustLName && aCartItem.CheckedOut == false && aCartItem.OrderID != "0"
                                  select aCartItem;

                        Table tblShoppingCart = Table.LoadTable(dynDBClient, "ShoppingCart");
                        Table tblCreditCard = Table.LoadTable(dynDBClient, "CreditCard");
                        foreach (ShoppingCart aCartItem in theCart)
                        {
                            Document docCartItem = new Document();
                            Document docPurchase = new Document();
                            docCartItem["OrderID"] = aCartItem.OrderID;
                            docCartItem["CheckedOut"] = 1;
                            docPurchase["ChargeID"] = (iCountAllPurchases++).ToString();
                            docPurchase["CardNumber"] = strCCNum;
                            docPurchase["Amount"] = aCartItem.TotalCost;
                            docPurchase["CardName"] = strCCName;
                            docPurchase["CustomerName"] = string.Concat(strCustFName, " ", strCustLName);
                            docPurchase["Expiration"] = strExpDate;
                            docPurchase["ItemDescription"] = aCartItem.ProductDescription;
                            docPurchase["Verification"] = strVerifyCode;
                            docPurchase["Merchant"] = aCartItem.StoreName;
                            docPurchase["PurchaseDate"] = strOrderDate;
                            docPurchase["NewCharge"] = 1;
                            await tblCreditCard.PutItemAsync(docPurchase);
                            //  await tblShoppingCart.UpdateItemAsync(docCartItem);
                            await tblShoppingCart.DeleteItemAsync(docCartItem);
                        }

                        
                          Intent intentDeliveryPickup = new Intent(this, typeof(DeliveryPickupActivity));

                          StartActivity(intentDeliveryPickup);
                    }
                });
                dlgCheckout.SetNegativeButton("NO", delegate { });
                dlgCheckout.Show();
            };
            btnEnterOrder.Click += async (sender, e) =>
            {

                AsyncSearch<ShoppingCart> listUpdateDataCartItems = dynContext.FromScanAsync<ShoppingCart>(new ScanOperationConfig()
                {

                    ConsistentRead = true
                });

                List<ShoppingCart>  dataUpdateCartItems =await listUpdateDataCartItems.GetRemainingAsync();
                int iOrderCount = dataUpdateCartItems.Count;
                string strDescription = edtItemDescription.Text.Trim();
                string strPrice = edtPrice.Text.Trim();
                string strQuant = edtQuant.Text.Trim();
                if (strDescription == "")
                {
                    Toast.MakeText(this, "PLEASE ENTER A PRODUCT DESCRIPTION", ToastLength.Long).Show();
                    return;
                }
                Double dblNum = 0.00;
                if (Double.TryParse(strPrice, out dblNum)==false)
                {
                    Toast.MakeText(this, "PLEASE ENTER PRICE AS A REAL NUMBER", ToastLength.Long).Show();
                    return;
                }
                int iNum = 0;
                if (int.TryParse(strQuant, out iNum) == false)
                {
                    Toast.MakeText(this, "PLEASE ENTER QUANTITY AS AN INTEGER NUMBER", ToastLength.Long).Show();
                    return;
                }
                else if (Convert.ToInt32(strQuant) <=0 )
                {
                    Toast.MakeText(this, "PLEASE ENTER A POSITIVE NUMBER (>=1)", ToastLength.Long).Show();
                    return;

                }
                Table tblCart = Table.LoadTable(dynDBClient, "ShoppingCart");
                Document docMerchandise = new Document();
                double dblUnitPrice = Convert.ToDouble(strPrice);
                
                int iQuant = Convert.ToInt32(strQuant);
                double dblTotalCost = Convert.ToDouble(iQuant) * dblUnitPrice;
                docMerchandise["OrderID"] = iOrderCount.ToString();
                docMerchandise["ProductDescription"] = strDescription;
                docMerchandise["CustomerFname"] = strCustFName;
                docMerchandise["CustomerLname"] = strCustLName;
                docMerchandise["StoreName"] = strStoreName;
                docMerchandise["UnitPrice"] = dblUnitPrice;
                docMerchandise["Quantity"] = iQuant;
                docMerchandise["TotalCost"] = dblTotalCost;
                docMerchandise["OrderDate"] = strOrderDate;
                docMerchandise["CheckedOut"] = 0;
                await tblCart.PutItemAsync(docMerchandise);
                listCartItems(dataCartItems, grdShoppingCart);
              //  double dblRunningTotal += dblTotalCost;
                edtItemDescription.Text = "";
                edtPrice.Text = "";
                edtQuant.Text = "";
              //  dblTotalPurchase = dblRunningTotal;
            };
            btnBack.Click += (sender, e) =>
            {
                Intent intentItems = new Intent(this, typeof(ItemsActivity));
                intentItems.PutExtra("CustomerFname", strCustFName);
                intentItems.PutExtra("CustomerLname", strCustLName);
                intentItems.PutExtra("SelectedStore", strStoreName);
                intentItems.PutExtra("UserZipCode", strZipCode);
                intentItems.PutExtra("OrderDate", strOrderDate);
                intentItems.PutExtra("ProductType", strSelectItem);
                StartActivity(intentItems);
            };

        }
        public async void listCartItems(List<ShoppingCart> dataCartItems,  GridLayout grdShoppingCart)
        {
            dbConfig.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamobdb/";
            dbConfig.AuthenticationRegion = "dynamodb.us-east-1.amazonaws.com";
            dbConfig.RegionEndpoint = RegionEndpoint.USEast1;

            AmazonDynamoDBClient dynDBClient = new AmazonDynamoDBClient("AKIAIMDIMZSEHYRAI6CQ", "6B2FRtd4JZiwq2iqiQJOmJPytboQ7EDOb08xovN3", dbConfig.RegionEndpoint);


            //dynDBClient.Config.ServiceURL= "https://console.aws.amazon.com/dynamodb/"; 
            dynDBClient.Config.ServiceURL = "https://026821060357.signin.aws.amazon.com/console/dynamodb/";
            dynDBClient.Config.RegionEndpoint = RegionEndpoint.USEast1;
            DynamoDBContext dynContext = new DynamoDBContext(dynDBClient);

            AsyncSearch<ShoppingCart> listCartItems = dynContext.FromScanAsync<ShoppingCart>(new ScanOperationConfig()
            {

                ConsistentRead = true
            });

            dataCartItems = await listCartItems.GetRemainingAsync();



            var theCart = from aCartItem in dataCartItems
                          where aCartItem.CustomerFname == strCustFName && aCartItem.CustomerLname==strCustLName && aCartItem.CheckedOut==false
                          select aCartItem;
            grdShoppingCart.RemoveAllViews();
            foreach (ShoppingCart cartItem in theCart)
            {
                CheckBox tvCartItem= new CheckBox(this);
                string strCartItemDesc = cartItem.ProductDescription;
                double dblCartItemUnitPrice = cartItem.UnitPrice;
                int iCartItemQuant = cartItem.Quantity;
                double dblCartItemTotalCost = cartItem.TotalCost;
                tvCartItem.Text = string.Format("{0} qty. {1} @ ${2} = {3}",strCartItemDesc,iCartItemQuant,dblCartItemUnitPrice,dblCartItemTotalCost);
                tvCartItem.SetTextSize(Android.Util.ComplexUnitType.Dip, 10f);
                tvCartItem.SetTextColor(Android.Graphics.Color.Black);
                tvCartItem.SetBackgroundColor(Android.Graphics.Color.White);
                tvCartItem.SetPadding(20, 5, 20, 5);
                tvCartItem.TextAlignment = TextAlignment.ViewStart;
                tvCartItem.SetWidth(1200);
                tvCartItem.SetBackgroundResource(Resource.Drawable.StoreName);
                
                grdShoppingCart.AddView(tvCartItem);


            }
            dblTotalPurchase = dataCartItems.Sum<ShoppingCart>(x => x.TotalCost);
        }
    }
}