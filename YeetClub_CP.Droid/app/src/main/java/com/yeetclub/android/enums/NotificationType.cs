using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.enums
{

/// <summary>
/// @author shuklaalok7
/// @since 9/12/2016
/// </summary>
public enum NotificationType {
    LIKE("pushLike"), REPLY("pushReply");

    private string pushFunction;

    NotificationType(string pushFunction) {
        this.pushFunction = pushFunction;
    }

    /// <summary>
    /// @param searchTerm
    /// @return
    /// </summary>
    public static NotificationType search(string searchTerm) {
        if (searchTerm == null || searchTerm.IsEmpty()) {
            return null;
        }

        string curatedSearchTerm = searchTerm.ToUpper().Replace("TYPE", "");

        foreach (NotificationType group in NotificationType.values()) {
            if (curatedSearchTerm.equalsIgnoreCase(group.name())) {
                return group;
            }
        }
        return null;
    }

    /// <summary>
    /// @return The key to be used for grouping the notifications
    /// </summary>
    public string getKey() {
        return "com.yeetclub." + this.name();
    }

    public string getPushFunction() {
        return pushFunction;
    }

}
}
