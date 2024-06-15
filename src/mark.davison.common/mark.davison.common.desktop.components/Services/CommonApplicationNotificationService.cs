﻿namespace mark.davison.common.desktop.components.Services;

public class CommonApplicationNotificationService : ICommonApplicationNotificationService
{
    public event EventHandler AuthenticationStateChanged = default!;

    public void NotifyAuthenticationStateChanged()
    {
        AuthenticationStateChanged?.Invoke(this, EventArgs.Empty);
    }
}
