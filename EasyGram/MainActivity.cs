using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content.PM;
using Android;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Support.V4.Content;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Android.Content;

namespace EasyGram
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme",MainLauncher =true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        static readonly int INTERNET = 0;
        static readonly int READ_EXTERNAL = 1;
        static readonly int WRITE_EXTERNAL = 1;

        View layout;
        Button btn_download;
        EditText txt_url;
        TextView tv_caption;
        
        string type = string.Empty;
        string image = string.Empty;
        string video = string.Empty;
        string caption = string.Empty;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            layout = FindViewById<View>(Resource.Id.layout_controls);
            btn_download = FindViewById<Button>(Resource.Id.btn_download);
            txt_url = FindViewById<EditText>(Resource.Id.txt_url);
            tv_caption = FindViewById<TextView>(Resource.Id.txt_caption);
            tv_caption.LongClick += Tv_caption_LongClick;
            btn_download.Click += Btn_download_Click;
            txt_url.TextChanged += Txt_url_TextChanged;
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
            {
                checkInternet();
                checkReadExternal();
                checkWriteExternal();
            }
        }

        private void Btn_download_Click(object sender, EventArgs e)
        {
            if (checkPermissions())
            {
                string name = Path.GetRandomFileName().Replace(".", "").Substring(0, 6);
             
                if (type == "video")
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadProgressChanged += Client_DownloadProgressChanged;
                        client.DownloadFileCompleted += delegate
                        {
                            Toast.MakeText(this, "Downloaded", ToastLength.Short).Show();
                            btn_download.Enabled = true;
                            btn_download.Text = "Download";
                        };

                        client.DownloadFileAsync(new Uri(video), Android.OS.Environment.ExternalStorageDirectory + "//" + Android.OS.Environment.DirectoryDownloads + "//" + name + ".mp4");
                    }
                }
                if (type == "image")
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadProgressChanged += Client_DownloadProgressChanged;
                        client.DownloadFileCompleted += delegate
                        {
                            Toast.MakeText(this, "Downloaded", ToastLength.Short).Show();
                            btn_download.Enabled = true;
                            btn_download.Text = "Download";
                        };

                        client.DownloadFileAsync(new Uri(image), Android.OS.Environment.ExternalStorageDirectory + "//" + Android.OS.Environment.DirectoryDownloads + "//" + name + ".jpg");
                    }
                }

            }
            else
            {
                if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                {
                    checkInternet();
                    checkReadExternal();
                    checkWriteExternal();
                }
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            btn_download.Enabled = false;
            btn_download.Text = "Downloading...";
        }

        private void Txt_url_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (checkPermissions())
                {

                   
                    WebClient client = new WebClient();
                    Stream stm = client.OpenRead(txt_url.Text);
                    StreamReader str = new StreamReader(stm);

                    string codes = str.ReadToEnd();
                    str.Close();
                    stm.Close();
                    stm = client.OpenRead(txt_url.Text);
                    str = new StreamReader(stm);
                    string line = "";


                    if (codes.Contains("<meta name=\"medium\" content=\"image\" />"))
                    {

                        type = "image";


                    }
                    else if (codes.Contains("<meta name=\"medium\" content=\"video\" />"))
                    {
                        type = "video";

                    }
                    while (!str.EndOfStream)
                    {
                        line = str.ReadLine();
                        if (line.Contains("<meta property=\"og:image\""))
                        {
                            image = line;
                        }
                        if (line.Contains("<meta property=\"og:title\""))
                        {
                            caption = line;
                        }
                        if (type == "video" && line.Contains("<meta property=\"og:video\""))
                        {
                            video = line;
                            video = video.Substring(39);
                            video = video.Trim("\" /> ".ToCharArray());
                        }
                    }
                    image = image.Substring(39);
                    image = image.Trim("\" /> ".ToCharArray());
                    caption = caption.Substring(calc_captionstart(caption) + 1);
                    caption = caption.Trim("”\" /> ".ToCharArray());
                    tv_caption.Text = caption;
                    str.Close();
                    stm.Close();
                    if (type == "image")
                        btn_download.Text = "Download Image";

                    if (type == "video")
                        btn_download.Text = "Download Video";
                }
                else
                {
                    if (Build.VERSION.SdkInt >= Build.VERSION_CODES.M)
                    {
                        checkInternet();
                        checkReadExternal();
                        checkWriteExternal();
                    }
                }
            }
            catch
            {

            }
        }
        private int calc_captionstart(string capton)
        {
            int i = 0;
            for (i = 0; i < caption.Length; i++)
            {
                if (caption[i] == '“')
                    return i;
            }
            return i;
        }
        private bool checkPermissions()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted || ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted || ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted)
                return false;
            else
                return true;
        }

        private void checkWriteExternal()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                if (ShouldShowRequestPermissionRationale(Manifest.Permission.WriteExternalStorage))
                {
                    Snackbar.Make(layout, "Please allow requst Write External",
           Snackbar.LengthIndefinite).SetAction("ok", new Action<View>(delegate (View obj)
           {
               RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage }, WRITE_EXTERNAL);
           })).Show();

                }
                else
                {
                    RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage }, WRITE_EXTERNAL);
                }
            }
        }

        private void checkReadExternal()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
            {
                if (ShouldShowRequestPermissionRationale(Manifest.Permission.ReadExternalStorage))
                {
                    Snackbar.Make(layout, "Please allow requst Read External",
           Snackbar.LengthIndefinite).SetAction("ok", new Action<View>(delegate (View obj) {
               RequestPermissions(new string[] { Manifest.Permission.ReadExternalStorage }, READ_EXTERNAL);
           })).Show();

                }
                else
                {
                    RequestPermissions(new string[] { Manifest.Permission.ReadExternalStorage }, READ_EXTERNAL);
                }
            }
        }

        private void checkInternet()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted)
            {
                if (ShouldShowRequestPermissionRationale(Manifest.Permission.Internet))
                {
                    Snackbar.Make(layout, "Please allow requst Internet",
                Snackbar.LengthIndefinite).SetAction("ok", new Action<View>(delegate (View obj) {
                    RequestPermissions(new string[] { Manifest.Permission.Internet }, INTERNET);
                })).Show();

                }
                else
                {
                    RequestPermissions(new string[] { Manifest.Permission.Internet }, INTERNET);
                }
            }

        }
        private void Tv_caption_LongClick(object sender, View.LongClickEventArgs e)
        {
            ClipboardManager clipboard = (ClipboardManager)GetSystemService(Context.ClipboardService);
            clipboard.Text = tv_caption.Text;
            Toast.MakeText(this,"Copy Caption to Clipboard!",ToastLength.Short).Show();
        }

       

     /*
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == INTERNET)
            {
                if(grantResults.Length==1 && grantResults[0] == Permission.Granted)
                {
                    Snackbar.Make(layout, "Permission Internet granted!", Snackbar.LengthShort).Show();

                }
                else
                {
                    Snackbar.Make(layout, "Permission Internet not granted!", Snackbar.LengthShort).Show();
                }
            }
            else if (requestCode == READ_EXTERNAL)
            {
                if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
                {
                    Snackbar.Make(layout, "Permission Read External Storage granted!", Snackbar.LengthShort).Show();

                }
                else
                {
                    Snackbar.Make(layout, "Permission Read External Storage not granted!", Snackbar.LengthShort).Show();
                }
            }
            else if (requestCode == WRITE_EXTERNAL)
            {
                if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
                {
                    Snackbar.Make(layout, "Permission Read External Storage granted!", Snackbar.LengthShort).Show();

                }
                else
                {
                    Snackbar.Make(layout, "Permission Read External Storage not granted!", Snackbar.LengthShort).Show();
                }
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
            
        }
    */
    }
    
}