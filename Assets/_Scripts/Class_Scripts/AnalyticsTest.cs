using System;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UnityConsent;
using UnityEngine.Analytics;
using Unity.Services.Analytics;

public class AnalyticsTest : MonoBehaviour
{
    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        CheckConsent();
    }

    private void CheckConsent()
    {
        EndUserConsent.SetConsentState(new ConsentState
        {
            AnalyticsIntent = ConsentStatus.Granted,
            AdsIntent = ConsentStatus.Granted
        });
    }

    public void ShopButtonPressed(string action)
    {
        ShopButtonEvent customEventAnalytics = new ShopButtonEvent()
        {
            eventName = "ShopButtonPressed",
            EventNameOwn = "ShopButtonPressed",
            pulsations = 1
        };

        AnalyticsService.Instance.RecordEvent(customEventAnalytics);

        AnalyticsService.Instance.Flush();
    }

}
 
public class ShopButtonEvent : Unity.Services.Analytics.Event
{
    public ShopButtonEvent() : base("ShopButtonPressed")
    {

    }

    public int pulsations { set { SetParameter("Pulsations", value); } }
    public string eventName { set { SetParameter("eventName", value); } }
    public string EventNameOwn { set { SetParameter("EventName", value); } }
}
