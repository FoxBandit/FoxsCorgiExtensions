using DG.Tweening;
using UnityEngine;

public class ComboWeaponIndicator : MonoBehaviour
{
    private Tween _localTween;
    public RectTransform minIndicator;
    public RectTransform primaryIndicator;

    public void StartIndicator(float minIndicatorScale, float comboTime)
    {
        ResetIndicator();
        minIndicator.transform.localScale = new Vector3(minIndicatorScale, minIndicatorScale, minIndicatorScale);

        _localTween = primaryIndicator.transform.DOScale(0, comboTime).SetEase(Ease.Linear);
    }

    public void ResetIndicator()
    {
        //End any executing animation
        _localTween?.Kill(true);

        //Reset the primary indicator
        primaryIndicator.transform.localScale = new Vector3(1, 1, 1);
    }
}