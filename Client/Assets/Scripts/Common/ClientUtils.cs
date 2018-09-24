﻿using System.Collections;
using UnityEngine;

class ClientUtils
{
    public static Color HTMLColorToColor(string htmlColor)
    {
        Color cl = new Color();
        ColorUtility.TryParseHtmlString(htmlColor, out cl);
        return cl;
    }

    public static void ChangePicture(Renderer rd, Texture tx)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        rd.GetPropertyBlock(mpb);
        mpb.SetTexture("_MainTex", tx);
        mpb.SetTexture("_EmissionMap", tx);
        rd.SetPropertyBlock(mpb);
    }

    public static void ChangePictureForCard(Renderer rd, int pictureID)
    {
        Texture tx = (Texture) Resources.Load("CardPictures/" + string.Format("{0:000}", pictureID));
        if (tx == null)
        {
            Debug.LogError("所选卡片没有图片资源：" + pictureID);
            tx = (Texture) Resources.Load("CardPictures/" + string.Format("{0:000}", 999));
        }

        ChangePicture(rd, tx);
    }

    public static void ChangeColor(Renderer rd, Color newColor)
    {
        if (!rd) return;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        rd.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", newColor);
        mpb.SetColor("_EmissionColor", newColor);
        rd.SetPropertyBlock(mpb);
    }

    public static void ChangeSlotColor(Renderer rd, SlotTypes slotTypes)
    {
        if (rd == null) return;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        rd.GetPropertyBlock(mpb);
        switch (slotTypes)
        {
            case SlotTypes.Weapon:
                mpb.SetColor("_Color", GameManager.Instance.Slot1Color);
                mpb.SetColor("_EmissionColor", GameManager.Instance.Slot1Color);
                break;
            case SlotTypes.Shield:
                mpb.SetColor("_Color", GameManager.Instance.Slot2Color);
                mpb.SetColor("_EmissionColor", GameManager.Instance.Slot2Color);
                break;
            case SlotTypes.Pack:
                mpb.SetColor("_Color", GameManager.Instance.Slot3Color);
                mpb.SetColor("_EmissionColor", GameManager.Instance.Slot3Color);
                break;
            case SlotTypes.MA:
                mpb.SetColor("_Color", GameManager.Instance.Slot4Color);
                mpb.SetColor("_EmissionColor", GameManager.Instance.Slot4Color);
                break;
            default:
                rd.enabled = false;
                break;
        }

        rd.SetPropertyBlock(mpb);
    }

    public static IEnumerator MoveGameObject(Transform obj, Vector3 oldPosition, Quaternion oldRotation, Vector3 oldScale, Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale, float duration, float rotateDuration)
    {
        obj.position = oldPosition;
        obj.rotation = oldRotation;

        float tick = 0;
        float tickRotate = 0;
        while (true)
        {
            if (tick > duration && tickRotate > rotateDuration) break;

            tick += Time.deltaTime;
            if (tick < duration)
            {
                obj.position = Vector3.Lerp(oldPosition, targetPosition, tick / duration);
                obj.localScale = Vector3.Lerp(oldScale, targetScale, tick / duration);
            }

            tickRotate += Time.deltaTime;
            if (tickRotate < rotateDuration)
            {
                obj.rotation = Quaternion.Slerp(oldRotation, targetRotation, tickRotate / rotateDuration);
            }

            yield return null;
        }

        obj.position = targetPosition;
        obj.rotation = targetRotation;
        obj.localScale = targetScale;
    }

    public struct PositionAndRotation
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public static float GetClipLength(Animator animator, string clip)
    {
        if (null == animator || string.IsNullOrEmpty(clip) || null == animator.runtimeAnimatorController)
            return 0;
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        AnimationClip[] tAnimationClips = ac.animationClips;
        if (null == tAnimationClips || tAnimationClips.Length <= 0) return 0;
        AnimationClip tAnimationClip;
        for (int tCounter = 0, tLen = tAnimationClips.Length; tCounter < tLen; tCounter++)
        {
            tAnimationClip = ac.animationClips[tCounter];
            if (null != tAnimationClip && tAnimationClip.name == clip)
                return tAnimationClip.length;
        }

        return 0F;
    }

    public static Color ChangeColorToWhite(Color color, float whiteRatio)
    {
        float r = color.r;
        float g = color.g;
        float b = color.b;

        float max = Mathf.Max(r, g, b);

        Color res = Color.Lerp(color, new Color(max, max, max, color.a), whiteRatio);
        return res;
    }
}