// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Markdown;

/// <summary>
/// The GitHub flavored markdown alert type.
/// For more information, see <a href="https://github.com/orgs/community/discussions/16925"></a>
/// </summary>
public enum AlertType
{
    /// <summary>
    /// Renders as a blue note alert with an icon that looks like an <c>i</c> in a circle.
    /// </summary>
    /// <remarks>
    /// Highlights information that users should take into account, even when skimming.
    /// </remarks>
    Note,

    /// <summary>
    /// Renders as a green tip alert with an icon that looks like a lightbulb.
    /// </summary>
    /// <remarks>
    /// Optional information to help a user be more successful.
    /// </remarks>
    Tip,

    /// <summary>
    /// Renders as a purple important alert with an icon that looks like an exclamation in an arrowed square callout.
    /// </summary>
    /// <remarks>
    /// Crucial information necessary for users to succeed.
    /// </remarks>
    Important,

    /// <summary>
    /// Renders as a orange warning alert with an icon that looks like a warning triangle.
    /// </summary>
    /// <remarks>
    /// Critical content demanding immediate user attention due to potential risks.
    /// </remarks>
    Warning,

    /// <summary>
    /// Renders as a red caution alert with an icon that looks like a stop sign.
    /// </summary>
    /// <remarks>
    /// Negative potential consequences of an action.
    /// </remarks>
    Caution
};
