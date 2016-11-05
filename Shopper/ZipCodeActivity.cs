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

namespace Shopper
{
    [Activity(Label = "ZipCodeActivity")]
    public class ZipCodeActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.ZipCode);
            Button btnToStores = FindViewById<Button>(Resource.Id.btnToStores);
            EditText edtZipCode= FindViewById<EditText>(Resource.Id.edtZipCode);

            btnToStores.Click += (sender, e) =>
            {
                string strZipPattern = @"^\d{5}$";
                string strZipInput = edtZipCode.Text;
                
                if (Regex.IsMatch(strZipInput,strZipPattern) )
                {
                    Intent intentStores = new Intent(this, typeof(StoresActivity));
                    intentStores.PutExtra("UserZipCode", strZipInput);
                    StartActivity(intentStores);
                    
                }
                else
                {
                    var dlgZipInputErr = new AlertDialog.Builder(this);
                    dlgZipInputErr.SetTitle("INPUT INVALID!");
                    dlgZipInputErr.SetMessage("Please enter a 5 digit zip code!");
                    dlgZipInputErr.SetNeutralButton("OK", delegate { });
                    dlgZipInputErr.Show();

                }
            };
        }
    }
}