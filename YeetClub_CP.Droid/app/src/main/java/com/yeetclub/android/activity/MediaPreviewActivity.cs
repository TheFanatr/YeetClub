using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.activity
{




public class MediaPreviewActivity : AppCompatActivity {

    private SubsamplingScaleImageView imageView;

    @SuppressLint("SetTextI18n")
    override protected void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);
        setContentView(R.layout.activity_media_preview);
        try {
            locateImageView();
        } catch (URISyntaxException | IOException e) {
            e.printStackTrace();
        }
    }

    override public bool onCreateOptionsMenu(Menu menu) {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.settings_media_preview, menu);
        return base.onCreateOptionsMenu(menu);
    }

    override public bool onOptionsItemSelected(MenuItem item) {
        if (item.getItemId() == R.id.main_action_rotate) {
            //rotating image
            imageView.setOrientation((imageView.getOrientation() + 90) % 360);
        }

        return base.onOptionsItemSelected(item);
    }

    private void locateImageView() , IOException {
        Bundle bundle = getIntent().getExtras();
        if (bundle != null) {
            if (bundle.getString("imageUrl") != null) {
                string imageUrl = bundle.getString("imageUrl");
                Log.w(GetType().ToString(), imageUrl);

                imageView = (SubsamplingScaleImageView) findViewById(R.id.image);

                try {
                    Url url = new Url(imageUrl);
                    HttpURLConnection connection = (HttpURLConnection) url.openConnection();
                    connection.setDoInput(true);
                    connection.connect();
                    StreamReader input = connection.getInputStream();
                    Bitmap myBitmap = BitmapFactory.decodeStream(input);

                    imageView.setImage(ImageSource.bitmap(myBitmap));

                } catch (IOException e) {
                    // Log exception
                    Log.w(GetType().ToString(), e);
                }

            }
        }
    }
}
}
