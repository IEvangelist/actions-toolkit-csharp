// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkitSharp.Core.EnvironmentVariables;

/// <summary>
/// A collection of environment variable keys, commonly used by GitHub Actions.
/// </summary>
/// <seealso href="https://docs.github.com/en/actions/learn-github-actions/variables#default-environment-variables"/>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1707:Identifiers should not contain underscores",
    Justification = "These values correspond to environment variables and I want them to match exactly.")]
#if ACTIONS_CORE_ENVIRONMENTVARIABLES_PUBLIC
public
#else
internal
#endif
static class Keys
{
    // Variables listed on https://docs.github.com/en/actions/learn-github-actions/variables#default-environment-variables

    /// <summary>
    /// The environment variable key: <c>GITHUB_ACTION</c>.
    /// <para>The name of the action currently running, or the <see href="https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions#jobsjob_idstepsid"><c>id</c></see> of a step. For example, for an action, <c>__repo-owner_name-of-action-repo</c>.</para>
    /// <para>GitHub removes special characters, and uses the name <c>__run</c> when the current step runs a script without an <c>id</c>. If you use the same script or action more than once in the same job, the name will include a suffix that consists of the sequence number preceded by an underscore. For example, the first script you run will have the name <c>__run</c>, and the second script will be named <c>__run_2</c>. Similarly, the second invocation of <c>actions/checkout</c> will be <c>actionscheckout2</c>.</para>
    /// </summary>
    public const string GITHUB_ACTION = nameof(GITHUB_ACTION);

    /// <summary>
    /// The environment variable key: <c>GITHUB_ACTION_PATH</c>.
    /// <para>The path where an action is located. This property is only supported in composite actions. You can use this path to change directories to where the action is located and access other files in that same repository. For example, <c>/home/runner/work/_actions/repo-owner/name-of-action-repo/v1</c>.</para>
    /// </summary>
    public const string GITHUB_ACTION_PATH = nameof(GITHUB_ACTION_PATH);

    /// <summary>
    /// The environment variable key: <c>GITHUB_ACTION_REPOSITORY</c>.
    /// <para>For a step executing an action, this is the owner and repository name of the action. For example, <c>actions/checkout</c>.</para>
    /// </summary>
    public const string GITHUB_ACTION_REPOSITORY = nameof(GITHUB_ACTION_REPOSITORY);

    /// <summary>
    /// The environment variable key: <c>GITHUB_ACTIONS</c>.
    /// <para>Always set to <c>true</c> when GitHub Actions is running the workflow. You can use this variable to differentiate when tests are being run locally or by GitHub Actions.</para>
    /// </summary>
    public const string GITHUB_ACTIONS = nameof(GITHUB_ACTIONS);

    /// <summary>
    /// The environment variable key: <c>GITHUB_ACTOR</c>.
    /// <para>The name of the person or app that initiated the workflow. For example, <c>octocat</c>.</para>
    /// </summary>
    public const string GITHUB_ACTOR = nameof(GITHUB_ACTOR);

    /// <summary>
    /// The environment variable key: <c>GITHUB_ACTOR_ID</c>.
    /// <para>The account ID of the person or app that triggered the initial workflow run. For example, <c>1234567</c>. Note that this is different from the actor username.</para>
    /// </summary>
    public const string GITHUB_ACTOR_ID = nameof(GITHUB_ACTOR_ID);

    /// <summary>
    /// The environment variable key: <c>GITHUB_API_URL</c>.
    /// <para>Returns the API URL. For example: <c>https://api.github.com</c>.</para>
    /// </summary>
    public const string GITHUB_API_URL = nameof(GITHUB_API_URL);

    /// <summary>
    /// The environment variable key: <c>GITHUB_BASE_REF</c>.
    /// <para>The name of the base ref or target branch of the pull request in a workflow run. This is only set when the event that triggers a workflow run is either <c>pull_request</c> or <c>pull_request_target</c>. For example, <c>main</c>.</para>
    /// </summary>
    public const string GITHUB_BASE_REF = nameof(GITHUB_BASE_REF);

    /// <summary>
    /// The environment variable key: <c>GITHUB_ENV</c>.
    /// <para>The path on the runner to the file that sets variables from workflow commands. The path to this file is unique to the current step and changes for each step in a job. For example, <c>/home/runner/work/_temp/_runner_file_commands/set_env_87406d6e-4979-4d42-98e1-3dab1f48b13a</c>. For more information, see <a href="https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#setting-an-environment-variable">"Workflow commands for GitHub Actions"</a>.</para>
    /// </summary>
    public const string GITHUB_ENV = nameof(GITHUB_ENV);

    /// <summary>
    /// The environment variable key: <c>GITHUB_EVENT_NAME</c>.
    /// <para>The name of the event that triggered the workflow. For example, <c>workflow_dispatch</c>.</para>
    /// </summary>
    public const string GITHUB_EVENT_NAME = nameof(GITHUB_EVENT_NAME);

    /// <summary>
    /// The environment variable key: <c>GITHUB_EVENT_PATH</c>.
    /// <para>The path to the file on the runner that contains the full event webhook payload. For example, <c>/github/workflow/event.json</c>.</para>
    /// </summary>
    public const string GITHUB_EVENT_PATH = nameof(GITHUB_EVENT_PATH);

    /// <summary>
    /// The environment variable key: <c>GITHUB_GRAPHQL_URL</c>.
    /// <para>Returns the GraphQL API URL. For example: <c>https://api.github.com/graphql</c>.</para>
    /// </summary>
    public const string GITHUB_GRAPHQL_URL = nameof(GITHUB_GRAPHQL_URL);

    /// <summary>
    /// The environment variable key: <c>GITHUB_HEAD_REF</c>.
    /// <para>The head ref or source branch of the pull request in a workflow run. This property is only set when the event that triggers a workflow run is either <c>pull_request</c> or <c>pull_request_target</c>. For example, <c>feature-branch-1</c>.</para>
    /// </summary>
    public const string GITHUB_HEAD_REF = nameof(GITHUB_HEAD_REF);

    /// <summary>
    /// The environment variable key: <c>GITHUB_JOB</c>.
    /// <para>The <see href="https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions#jobsjob_id">job_id</see> of the current job. For example, <c>greeting_job</c>.</para>
    /// </summary>
    public const string GITHUB_JOB = nameof(GITHUB_JOB);

    /// <summary>
    /// The environment variable key: <c>GITHUB_OUTPUT</c>.
    /// <para>The path on the runner to the file that sets the current step's outputs from workflow commands. The path to this file is unique to the current step and changes for each step in a job. For example, <c>/home/runner/work/_temp/_runner_file_commands/set_output_a50ef383-b063-46d9-9157-57953fc9f3f0</c>. For more information, see <a href="https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#setting-an-output-parameter">"Workflow commands for GitHub Actions"</a>.</para>
    /// </summary>
    public const string GITHUB_OUTPUT = nameof(GITHUB_OUTPUT);

    /// <summary>
    /// The environment variable key: <c>GITHUB_PATH</c>.
    /// <para>The path on the runner to the file that sets system <c>PATH</c> variables from workflow commands. The path to this file is unique to the current step and changes for each step in a job. For example, <c>/home/runner/work/_temp/_runner_file_commands/add_path_899b9445-ad4a-400c-aa89-249f18632cf5</c>. For more information, see see <a href="https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#adding-a-system-path">"Workflow commands for GitHub Actions"</a>.</para>
    /// </summary>
    public const string GITHUB_PATH = nameof(GITHUB_PATH);

    /// <summary>
    /// The environment variable key: <c>GITHUB_REF</c>.
    /// <para>The fully-formed ref of the branch or tag that triggered the workflow run. For workflows triggered by <c>push</c>, this is the branch or tag ref that was pushed. For workflows triggered by <c>pull_request</c>, this is the pull request merge branch. For workflows triggered by <c>release</c>, this is the release tag created. For other triggers, this is the branch or tag ref that triggered the workflow run. This is only set if a branch or tag is available for the event type. The ref given is fully-formed, meaning that for branches the format is <c>refs/heads/&lt;branch_name&gt;</c>, for pull requests it is <c>refs/pull/&lt;pr_number&gt;/merge</c>, and for tags it is <c>refs/tags/&lt;tag_name&gt;</c>. For example, <c>refs/heads/feature-branch-1</c>.</para>
    /// </summary>
    public const string GITHUB_REF = nameof(GITHUB_REF);

    /// <summary>
    /// The environment variable key: <c>GITHUB_REF_NAME</c>.
    /// <para>The short ref name of the branch or tag that triggered the workflow run. This value matches the branch or tag name shown on GitHub. For example, <c>feature-branch-1</c>.</para>
    /// <para>For pull requests, the format is <c>&lt;pr_number&gt;/merge</c>.</para>
    /// </summary>
    public const string GITHUB_REF_NAME = nameof(GITHUB_REF_NAME);

    /// <summary>
    /// The environment variable key: <c>GITHUB_REF_PROTECTED</c>.
    /// <para><c>true</c> if branch protections or <a href="https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/managing-rulesets-for-a-repository">rulesets</a> are configured for the ref that triggered the workflow run.</para>
    /// </summary>
    public const string GITHUB_REF_PROTECTED = nameof(GITHUB_REF_PROTECTED);

    /// <summary>
    /// The environment variable key: <c>GITHUB_REF_TYPE</c>.
    /// <para>The type of ref that triggered the workflow run. Valid values are <c>branch</c> or <c>tag</c>.</para>
    /// </summary>
    public const string GITHUB_REF_TYPE = nameof(GITHUB_REF_TYPE);

    /// <summary>
    /// The environment variable key: <c>GITHUB_REPOSITORY</c>.
    /// <para>The owner and repository name. For example, <c>octocat/Hello-World</c>.</para>
    /// </summary>
    public const string GITHUB_REPOSITORY = nameof(GITHUB_REPOSITORY);

    /// <summary>
    /// The environment variable key: <c>GITHUB_REPOSITORY_ID</c>.
    /// <para>The ID of the repository. For example, <c>123456789</c>. Note that this is different from the repository name.</para>
    /// </summary>
    public const string GITHUB_REPOSITORY_ID = nameof(GITHUB_REPOSITORY_ID);

    /// <summary>
    /// The environment variable key: <c>GITHUB_REPOSITORY_OWNER</c>.
    /// <para>The repository owner's name. For example, <c>octocat</c>.</para>
    /// </summary>
    public const string GITHUB_REPOSITORY_OWNER = nameof(GITHUB_REPOSITORY_OWNER);

    /// <summary>
    /// The environment variable key: <c>GITHUB_REPOSITORY_OWNER_ID</c>.
    /// <para>The repository owner's account ID. For example, <c>1234567</c>. Note that this is different from the owner's name.</para>
    /// </summary>
    public const string GITHUB_REPOSITORY_OWNER_ID = nameof(GITHUB_REPOSITORY_OWNER_ID);

    /// <summary>
    /// The environment variable key: <c>GITHUB_RETENTION_DAYS</c>.
    /// <para>The number of days that workflow run logs and artifacts are kept. For example, <c>90</c>.</para>
    /// </summary>
    public const string GITHUB_RETENTION_DAYS = nameof(GITHUB_RETENTION_DAYS);

    /// <summary>
    /// The environment variable key: <c>GITHUB_RUN_ATTEMPT</c>.
    /// <para>A unique number for each attempt of a particular workflow run in a repository. This number begins at 1 for the workflow run's first attempt, and increments with each re-run. For example, <c>3</c>.</para>
    /// </summary>
    public const string GITHUB_RUN_ATTEMPT = nameof(GITHUB_RUN_ATTEMPT);

    /// <summary>
    /// The environment variable key: <c>GITHUB_RUN_ID</c>.
    /// <para>A unique number for each workflow run within a repository. This number does not change if you re-run the workflow run. For example, <c>1658821493</c>.</para>
    /// </summary>
    public const string GITHUB_RUN_ID = nameof(GITHUB_RUN_ID);

    /// <summary>
    /// The environment variable key: <c>GITHUB_RUN_NUMBER</c>.
    /// <para>A unique number for each run of a particular workflow in a repository. This number begins at 1 for the workflow's first run, and increments with each new run. This number does not change if you re-run the workflow run. For example, <c>3</c>.</para>
    /// </summary>
    public const string GITHUB_RUN_NUMBER = nameof(GITHUB_RUN_NUMBER);

    /// <summary>
    /// The environment variable key: <c>GITHUB_SERVER_URL</c>.
    /// <para>The URL of the GitHub server. For example: <c>https://github.com</c>.</para>
    /// </summary>
    public const string GITHUB_SERVER_URL = nameof(GITHUB_SERVER_URL);

    /// <summary>
    /// The environment variable key: <c>GITHUB_SHA</c>.
    /// <para>The commit SHA that triggered the workflow. The value of this commit SHA depends on the event that triggered the workflow. For more information, see <a href="https://docs.github.com/en/actions/using-workflows/events-that-trigger-workflows">"Events that trigger workflows"</a>. For example, <c>ffac537e6cbbf934b08745a378932722df287a53</c>.</para>
    /// </summary>
    public const string GITHUB_SHA = nameof(GITHUB_SHA);

    /// <summary>
    /// The environment variable key: <c>GITHUB_STEP_SUMMARY</c>.
    /// <para>The path on the runner to the file that contains job summaries from workflow commands. The path to this file is unique to the current step and changes for each step in a job. For example, <c>/home/runner/_layout/_work/_temp/_runner_file_commands/step_summary_1cb22d7f-5663-41a8-9ffc-13472605c76c</c>. For more information, see <a href="https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#adding-a-job-summary">"Workflow commands for GitHub Actions"</a>.</para>
    /// </summary>
    public const string GITHUB_STEP_SUMMARY = nameof(GITHUB_STEP_SUMMARY);

    /// <summary>
    /// The environment variable key: <c>GITHUB_TRIGGERING_ACTOR</c>.
    /// <para>The username of the user that initiated the workflow run. If the workflow run is a re-run, this value may differ from <c>github.actor</c>. Any workflow re-runs will use the privileges of <c>github.actor</c>, even if the actor initiating the re-run (<c>github.triggering_actor</c>) has different privileges.</para>
    /// </summary>
    public const string GITHUB_TRIGGERING_ACTOR = nameof(GITHUB_TRIGGERING_ACTOR);

    /// <summary>
    /// The environment variable key: <c>GITHUB_WORKFLOW</c>.
    /// <para>The name of the workflow. For example, <c>My test workflow</c>. If the workflow file doesn't specify a <c>name</c>, the value of this variable is the full path of the workflow file in the repository.</para>
    /// </summary>
    public const string GITHUB_WORKFLOW = nameof(GITHUB_WORKFLOW);

    /// <summary>
    /// The environment variable key: <c>GITHUB_WORKFLOW_REF</c>.
    /// <para>The ref path to the workflow. For example, <c>octocat/hello-world/.github/workflows/my-workflow.yml@refs/heads/my_branch</c>.</para>
    /// </summary>
    public const string GITHUB_WORKFLOW_REF = nameof(GITHUB_WORKFLOW_REF);

    /// <summary>
    /// The environment variable key: <c>GITHUB_WORKFLOW_SHA</c>.
    /// <para>The commit SHA for the workflow file.</para>
    /// </summary>
    public const string GITHUB_WORKFLOW_SHA = nameof(GITHUB_WORKFLOW_SHA);

    /// <summary>
    /// The environment variable key: <c>GITHUB_WORKSPACE</c>.
    /// <para>The default working directory on the runner for steps, and the default location of your repository when using the <a href="https://github.com/actions/checkout"><c>checkout</c></a> action. For example, <c>/home/runner/work/my-repo-name/my-repo-name</c>.</para>
    /// </summary>
    public const string GITHUB_WORKSPACE = nameof(GITHUB_WORKSPACE);

    /// <summary>
    /// The environment variable key: <c>RUNNER_ARCH</c>.
    /// <para>The architecture of the runner executing the job. Possible values are <c>X86</c>, <c>X64</c>, <c>ARM</c>, or <c>ARM64</c>.</para>
    /// </summary>
    public const string RUNNER_ARCH = nameof(RUNNER_ARCH);

    /// <summary>
    /// The environment variable key: <c>RUNNER_DEBUG</c>.
    /// <para>This is set only if <a href="https://docs.github.com/en/actions/monitoring-and-troubleshooting-workflows/enabling-debug-logging">debug logging</a> is enabled, and always has the value of <c>1</c>. It can be useful as an indicator to enable additional debugging or verbose logging in your own job steps.</para>
    /// </summary>
    public const string RUNNER_DEBUG = nameof(RUNNER_DEBUG);

    /// <summary>
    /// The environment variable key: <c>RUNNER_NAME</c>.
    /// <para>The name of the runner executing the job. This name may not be unique in a workflow run as runners at the repository and organization levels could use the same name. For example, <c>Hosted Agent</c></para>
    /// </summary>
    public const string RUNNER_NAME = nameof(RUNNER_NAME);

    /// <summary>
    /// The environment variable key: <c>RUNNER_OS</c>.
    /// <para>The operating system of the runner executing the job. Possible values are <c>Linux</c>, <c>Windows</c>, or <c>macOS</c>. For example, <c>Windows</c></para>
    /// </summary>
    public const string RUNNER_OS = nameof(RUNNER_OS);

    /// <summary>
    /// The environment variable key: <c>RUNNER_TEMP</c>.
    /// <para>The path to a temporary directory on the runner. This directory is emptied at the beginning and end of each job. Note that files will not be removed if the runner's user account does not have permission to delete them. For example, <c>D:\a\_temp</c></para>
    /// </summary>
    public const string RUNNER_TEMP = nameof(RUNNER_TEMP);

    /// <summary>
    /// The environment variable key: <c>RUNNER_TOOL_CACHE</c>.
    /// <para>The path to the directory containing preinstalled tools for GitHub-hosted runners. For more information, see <a href="https://docs.github.com/en/actions/using-github-hosted-runners/about-github-hosted-runners#supported-software">"Using GitHub-hosted runners"</a>. For example, <c>C:\hostedtoolcache\windows</c></para>
    /// </summary>
    public const string RUNNER_TOOL_CACHE = nameof(RUNNER_TOOL_CACHE);

    // Other variables

    /// <summary>
    /// The environment variable key: <c>PATH</c>.
    /// </summary>
    public const string PATH = nameof(PATH);

    /// <summary>
    /// The environment variable key: <c>ACTIONS_STEP_DEBUG</c>.
    /// </summary>
    public const string ACTIONS_STEP_DEBUG = nameof(ACTIONS_STEP_DEBUG);

    /// <summary>
    /// The environment variable key: <c>GITHUB_STATE</c>.
    /// </summary>
    public const string GITHUB_STATE = nameof(GITHUB_STATE);

    /// <summary>
    /// The environment variable key: <c>GITHUB_TOKEN</c>.
    /// </summary>
    public const string GITHUB_TOKEN = nameof(GITHUB_TOKEN);
}
