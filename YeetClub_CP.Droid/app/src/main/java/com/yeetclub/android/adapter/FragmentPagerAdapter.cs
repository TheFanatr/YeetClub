using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.adapter
{

/// <summary>
/// Created by @santafebound on 2016-09-29.
/// </summary>



public class FragmentPagerAdapter : FragmentStatePagerAdapter {
    private int mNumOfTabs;

    public FragmentPagerAdapter(FragmentManager fm, int NumOfTabs) {
        base(fm);
        this.mNumOfTabs = NumOfTabs;
    }

    override public Fragment getItem(int position) {

        switch (position) {
            case 0:
                return new FeedFragment();
            case 1:
                return new NotificationsFragment();
            case 2:
                return new UsersListFragment();
            default:
                return null;
        }
    }

    override public int getCount() {
        return mNumOfTabs;
    }
}
}
