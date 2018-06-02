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
using ZXing.Mobile;             //引用的库

namespace ParcelTracker
{
    [Activity(Label = "ScanActivity")]      //activityLabel
    public class ScanActivity : Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            MobileBarcodeScanner.Initialize(Application);       //条形码扫描初始化

            try
            {
                var scanner = new MobileBarcodeScanner();       //定义scanner

                //scanner.UseCustomOverlay = true;
                //scanner.BottomText = "Ensure the barcode is inside the viewfinder";
                //scanner.FlashButtonText = "Flash";
                //scanner.CancelButtonText = "Cancel";
                //scanner.Torch(true);
                //scanner.AutoFocus();

                var result = await scanner.Scan();              //获取scan的内容
                if (!string.IsNullOrEmpty(result.Text))         //判断内容是否为空
                {
                    Intent intent = new Intent();               //定义意向
                    intent.PutExtra("parcelNum", result.Text);  //传递快递单号
                    SetResult(Result.Ok, intent);               //返回意向
                    Finish();       //activity结束
                }
            }
            catch
            {
                Finish();           //activity结束
            }
        }

        //public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        //{
        //    if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
        //    {
        //        Finish();
        //    }
        //    return true;
        //}
    }
}