﻿using System.Globalization;
using Content.Server.Chat.Managers;
using Content.Server.Mind;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;

namespace Content.Server.Roles.Jobs;

public sealed class JobSystem : SharedJobSystem
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly MindSystem _minds = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MindComponent, MindRoleAddedEvent>(MindOnDoGreeting);
    }

    private void MindOnDoGreeting(EntityUid mindId, MindComponent component, ref MindRoleAddedEvent args)
    {
        if (args.Silent)
            return;

        if (!_minds.TryGetSession(mindId, out var session))
            return;

        if (!MindTryGetJob(mindId, out _, out var prototype))
            return;

        _chat.DispatchServerMessage(session, Loc.GetString("job-greet-introduce-job-name",
            ("jobName", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(prototype.LocalizedName))));

        if (prototype.RequireAdminNotify)
            _chat.DispatchServerMessage(session, Loc.GetString("job-greet-important-disconnect-admin-notify"));

        _chat.DispatchServerMessage(session, Loc.GetString("job-greet-supervisors-warning", ("jobName", prototype.LocalizedName), ("supervisors", Loc.GetString(prototype.Supervisors))));
    }

    public void MindAddJob(EntityUid mindId, string jobPrototypeId)
    {
        if (MindHasJobWithId(mindId, jobPrototypeId))
            return;

        _roles.MindAddRole(mindId, new JobComponent { PrototypeId = jobPrototypeId });
    }
}
