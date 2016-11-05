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

namespace Shopper
{
    [Activity(Label = "GoShoppingActivity")]
    public class GoShoppingActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.ShoppingDelivering);
            Button btnGo = FindViewById<Button>(Resource.Id.btnGo);
            RadioButton rbBuying = FindViewById<RadioButton>(Resource.Id.rbBuying);
            RadioButton rbDelivering = FindViewById<RadioButton>(Resource.Id.rbDelivering);
            RadioGroup radioGroup1= FindViewById<RadioGroup>(Resource.Id.radioGroup1);
           
            btnGo.Click += (sender, e) =>
            {
                RadioButton rbShoppingChoice = new RadioButton(this);
                rbShoppingChoice = (rbBuying.Checked) ? rbBuying : rbDelivering;
                switch (rbShoppingChoice.Text)
                {
                    case "Buying Items":
                        Intent intentZip = new Intent(this, typeof(ZipCodeActivity));
                        StartActivity(intentZip);
                        break;

                    case "Delivering Items":
                        Intent intentDelivNote = new Intent(this, typeof(DeliveryNotificationActivity));
                        StartActivity(intentDelivNote);
                        break;
                }
            };
            
        }
    }
}