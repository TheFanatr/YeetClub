using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.activity
{

/// <summary>
/// Created by @santafebound on 2016-09-29.
/// </summary>



public class MainActivity : AppCompatActivity {

    override protected void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main_fragment);
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        // HashSet typeface for action bar title
        TextView text = (TextView) findViewById(R.id.feed_title);
        Typeface tf = Typeface.createFromAsset(getAssets(), "fonts/Lato-Bold.ttf");
        text.setTypeface(tf);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M && (checkSelfPermission(android.Manifest.permission.WRITE_EXTERNAL_STORAGE) != PackageManager.PERMISSION_GRANTED)) {
            ActivityCompat.requestPermissions(this, new string[]{android.Manifest.permission.WRITE_EXTERNAL_STORAGE}, 1);
        } else {
            getFragmentManager();
        }

        createNavigationDrawer(savedInstanceState, toolbar);

        FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab);
        fab.setBackgroundTintList(ColorStateList.valueOf(Color.parseColor("#169cee")));
        fab.setOnClickListener(view -> {

            Intent intent = new Intent(MainActivity.this, YeetActivity.class);
            startActivity(intent);

        });
    }


    public FragmentManager getFragmentManager() {
        TabLayout tabLayout = (TabLayout) findViewById(R.id.tab_layout);
        tabLayout.addTab(tabLayout.newTab().setIcon(R.drawable.ic_tab_poo));
        tabLayout.addTab(tabLayout.newTab().setIcon(R.drawable.ic_tab_notification));
        tabLayout.addTab(tabLayout.newTab().setIcon(R.drawable.ic_tab_points));
        tabLayout.setTabGravity(TabLayout.GRAVITY_FILL);

        ViewPager viewPager = (ViewPager) findViewById(R.id.pager);
        PagerAdapter adapter = new FragmentPagerAdapter
                (getSupportFragmentManager(), tabLayout.getTabCount());
        viewPager.setAdapter(adapter);
        viewPager.addOnPageChangeListener(new TabLayout.TabLayoutOnPageChangeListener(tabLayout));
        tabLayout.setOnTabSelectedListener(new TabLayout.OnTabSelectedListener() {
            override public void onTabSelected(TabLayout.Tab tab) {
                viewPager.setCurrentItem(tab.getPosition());
            }

            override public void onTabUnselected(TabLayout.Tab tab) {

            }

            override public void onTabReselected(TabLayout.Tab tab) {

            }

        });

        viewPager.getAdapter().notifyDataSetChanged();

        Bundle bundle = getIntent().getExtras();
        if (bundle != null) {
            if (bundle.getString("fragment") != null) {
                if ("fragment2".Equals(bundle.getString("fragment"))) {
                /*Log.w(GetType().ToString(), bundle.getString("fragment"));*/
                    viewPager.setCurrentItem(1);
                } else {
                    viewPager.setCurrentItem(0);
                }
            }
        }

        return null;
    }


    override public bool onCreateOptionsMenu(Menu menu) {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.settings_feed, menu);
        return base.onCreateOptionsMenu(menu);
    }


    override public bool onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.action_settings:
                Intent intent = new Intent(this, UserSettingsActivity.class);
                startActivity(intent);
                return true;
            default:
                return base.onOptionsItemSelected(item);
        }
    }


    // Save AccountHeader result
    private AccountHeader headerResult = null;
    private Drawer result = null;


    private void createNavigationDrawer(Bundle savedInstanceState, Toolbar toolbar) {

        if (ParseUser.getCurrentUser().getParseFile("profilePicture") != null) {
            DrawerImageLoader.init(new AbstractDrawerImageLoader() {
                override public void set(ImageView imageView, Uri uri, Drawable placeholder) {
                    Picasso.with(imageView.getContext()).load(uri).placeholder(placeholder).into(imageView);
                    imageView.setScaleType(ImageView.ScaleType.FIT_XY);
                }

                override public void cancel(ImageView imageView) {
                    Picasso.with(imageView.getContext()).cancelRequest(imageView);
                }

            });

            string profilePicture = ParseUser.getCurrentUser().getParseFile("profilePicture").getUrl();
            setProfile(savedInstanceState, toolbar, profilePicture);
        } else {
            DrawerImageLoader.init(new AbstractDrawerImageLoader() {
                override public void set(ImageView imageView, Uri uri, Drawable placeholder) {
                    Picasso.with(imageView.getContext()).load(uri).placeholder(placeholder).into(imageView);
                    //imageView.setImageResource(R.drawable.ic_profile_pic_add);
                    imageView.setScaleType(ImageView.ScaleType.FIT_XY);
                }

                override public void cancel(ImageView imageView) {
                    Picasso.with(imageView.getContext()).cancelRequest(imageView);
                }

            });

            string profilePicture = "R.drawable.ic_profile_pic_add";
            setProfile(savedInstanceState, toolbar, profilePicture);
        }

    }


    private void setProfile(Bundle savedInstanceState, Toolbar toolbar, string profilePicture) {
        if (ParseUser.getCurrentUser()["name"] != null && ParseUser.getCurrentUser().getUsername() != null) {
            // If name and username exist, set to respective values from Parse
            string name = string.valueOf(ParseUser.getCurrentUser()["name"]);
            string username = string.valueOf(ParseUser.getCurrentUser().getUsername());
            setValues(savedInstanceState, toolbar, name, username, profilePicture);
        } else if (ParseUser.getCurrentUser()["name"] == null) {
            // If name does not exist, set name to nothing
            string name = "";
            string username = string.valueOf(ParseUser.getCurrentUser().getUsername());
            setValues(savedInstanceState, toolbar, name, username, profilePicture);
        }
    }


    private void setValues(Bundle savedInstanceState, Toolbar toolbar, string name, string username, string profilePicture) {
        IProfile<ProfileDrawerItem> profile;
        if (profilePicture.equalsIgnoreCase("R.drawable.ic_profile_pic_add")) {
            profile = new ProfileDrawerItem().withName(name).withEmail(username).withIcon(BitmapFactory.decodeResource(getResources(), R.drawable.ic_profile_pic_add)).withIdentifier(100);
        } else {
            profile = new ProfileDrawerItem().withName(name).withEmail(username).withIcon(profilePicture).withIdentifier(100);
        }
        // Create the AccountHeader
        headerResult = new AccountHeaderBuilder()
                .withActivity(this)
                .withTranslucentStatusBar(true)
                .withHeaderBackground(R.color.highlight)
                .addProfiles(
                        profile,
                        new ProfileSettingDrawerItem().withName(getString(R.string.drawer_item_edit_profile)).withIcon(new IconicsDrawable(this, GoogleMaterial.Icon.gmd_edit).actionBar().paddingDp(5).colorRes(R.color.material_drawer_primary_text)).withIdentifier(100000),
                        new ProfileSettingDrawerItem().withName(getString(R.string.drawer_item_logout)).withIcon(new IconicsDrawable(this, GoogleMaterial.Icon.gmd_exit_to_app).actionBar().paddingDp(5).colorRes(R.color.material_drawer_primary_text)).withIdentifier(100001)

                )
                .withOnAccountHeaderProfileImageListener(new AccountHeader.OnAccountHeaderProfileImageListener() {
                                                             override public bool onProfileImageClick(View view, IProfile profile, bool current) {
                                                                 Intent intent = new Intent(getApplicationContext(), UserProfileActivity.class);
                                                                 startActivity(intent);
                                                                 return false;
                                                             }

                                                             override public bool onProfileImageLongClick(View view, IProfile profile, bool current) {
                                                                 return false;
                                                             }
                                                         }
                )
                .withOnAccountHeaderListener(new AccountHeader.OnAccountHeaderListener() {
                    override public bool onProfileChanged(View view, IProfile profile, bool current) {
                        if (profile is IDrawerItem && profile.getIdentifier() == 100000) {
                            Intent intent = new Intent(getApplicationContext(), EditProfileActivity.class);
                            startActivity(intent);
                        } else if (profile is IDrawerItem && profile.getIdentifier() == 100001) {
                            ParseUser.logOut();
                            Intent logOut = new Intent(getApplicationContext(), DispatchActivity.class);
                            startActivity(logOut);
                        }

                        //false if you have not consumed the event and it should Close the drawer
                        return false;
                    }
                })
                .withSavedInstance(savedInstanceState)
                .build();

        // Create the Drawer
        result = new DrawerBuilder()
                .withActivity(this)
                .withToolbar(toolbar)
                .withHasStableIds(true)
                .withItemAnimator(new AlphaCrossFadeAnimator())
                .withAccountHeader(headerResult)
                .addDrawerItems(
                        new PrimaryDrawerItem().withName(R.string.drawer_item_profile).withIcon(FontAwesome.Icon.faw_user).withIdentifier(1).withSelectable(false),
                        new PrimaryDrawerItem().withName(R.string.drawer_item_create).withIcon(FontAwesome.Icon.faw_paint_brush).withIdentifier(2).withSelectable(false),
                        new PrimaryDrawerItem().withName(R.string.drawer_item_yaanich_news).withIcon(FontAwesome.Icon.faw_newspaper_o).withIdentifier(3).withSelectable(false),
                        new PrimaryDrawerItem().withName(R.string.drawer_item_my_groups).withIcon(FontAwesome.Icon.faw_users).withIdentifier(4).withSelectable(false),
                        new PrimaryDrawerItem().withName(R.string.drawer_item_settings).withIcon(FontAwesome.Icon.faw_cog).withIdentifier(5).withSelectable(false)
                )
                .withOnDrawerItemClickListener((view, position, drawerItem) -> {

                    if (drawerItem != null) {
                        Intent intent = null;
                        if (drawerItem.getIdentifier() == 1) {
                            intent = new Intent(this, UserProfileActivity.class);
                        } else if (drawerItem.getIdentifier() == 2) {
                            intent = new Intent(this, YeetActivity.class);
                        } else if (drawerItem.getIdentifier() == 3) {
                            intent = new Intent(this, RssActivity.class);
                        } else if (drawerItem.getIdentifier() == 4) {
                            intent = new Intent(this, GroupsActivity.class);
                        } else if (drawerItem.getIdentifier() == 5) {
                            intent = new Intent(this, UserSettingsActivity.class);
                        }
                        if (intent != null) {
                            this.startActivity(intent);
                        }
                    }
                    return false;
                })
                .withSavedInstance(savedInstanceState)
                .withShowDrawerOnFirstLaunch(true)
                .build();

        new RecyclerViewCacheUtil<IDrawerItem>().withCacheSize(2).apply(result.getRecyclerView(), result.getDrawerItems());

        if (savedInstanceState == null) {
            result.setSelection(21, false);
            headerResult.setActiveProfile(profile);
        }

        /*result.updateBadge(5, new StringHolder(10 + ""));*/
    }


    private OnCheckedChangeListener onCheckedChangeListener = new OnCheckedChangeListener() {
        override public void onCheckedChanged(IDrawerItem drawerItem, CompoundButton buttonView, bool isChecked) {
            if (drawerItem is Nameable) {
                Log.i("material-drawer", "DrawerItem: " + ((Nameable) drawerItem).getName() + " - toggleChecked: " + isChecked);
            } else {
                Log.i("material-drawer", "toggleChecked: " + isChecked);
            }
        }
    };


    override public void onRequestPermissionsResult(int requestCode, @NonNull string[] permissions, @NonNull int[] grantResults) {
        base.onRequestPermissionsResult(requestCode, permissions, grantResults);

        TextView checkFilePermission = (TextView) findViewById(R.id.checkFilePermission);

        if (grantResults[0] == PackageManager.PERMISSION_GRANTED) {
            getFragmentManager();
            checkFilePermission.setVisibility(View.GONE);
        } else {
            checkFilePermission.setVisibility(View.VISIBLE);
        }
    }


    public void enableFilePermission(View view) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M && (checkSelfPermission(android.Manifest.permission.WRITE_EXTERNAL_STORAGE) != PackageManager.PERMISSION_GRANTED)) {
            ActivityCompat.requestPermissions(this, new string[]{android.Manifest.permission.WRITE_EXTERNAL_STORAGE}, 1);
        } else {
            getFragmentManager();
        }
    }


    public void onResume() {
        base.onResume();
    }


    override protected void onSaveInstanceState(Bundle outState) {
        //Add the values which need to be saved from the drawer to the bundle
        outState = result.saveInstanceState(outState);
        //Add the values which need to be saved from the accountHeader to the bundle
        outState = headerResult.saveInstanceState(outState);
        base.onSaveInstanceState(outState);
    }


    override public void onBackPressed() {

        if (result != null && result.isDrawerOpen()) {
            result.closeDrawer();
        } else {
            base.onBackPressed();
        }
    }
}
}
