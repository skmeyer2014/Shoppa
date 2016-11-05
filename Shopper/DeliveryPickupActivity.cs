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
using static Android.Gestures.GestureOverlayView;

namespace Shopper
{
    [Activity(Label = "DeliveryPickupActivity")]
    public class DeliveryPickupActivity : Activity 
    {
       
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.DeliveryPickupLayout);
           // Button btnViewMap = FindViewById<Button>(Resource.Id.btnViewMap);
            Intent intentViewMap = new Intent(this, typeof(ViewMapActivity));
            Intent intentDelivery = new Intent(this, typeof(DeliveryActivity));
            LinearLayout llScreen = FindViewById<LinearLayout>(Resource.Id.llScreen);
            float flOrigX = 0.00f;
            float flNewX = 0.00f;
            llScreen.GenericMotion += (sender, e) =>
            {
                float flXTouch = e.Event.GetX();
                if (MotionEvent.ActionToString(MotionEventActions.Move)=="ACTION_MOVE")
                {
                    
                    if (flOrigX == 0.00f) flOrigX = e.Event.GetX();
                    flNewX = e.Event.GetX();
                }

               
                if (flNewX - flOrigX < 0)
                {
                    Toast.MakeText(this, "NOW TAKING YOU TO DELIVERY SCREEN...",ToastLength.Long).Show();
                    
                    StartActivity(intentDelivery);
                }
            };
            /*
            btnViewMap.Click += (sender, e) =>
            {
                StartActivity(intentViewMap);
            };
           */
        }
    } 
}