﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ClientUtils
{
    public static Color HTMLColorToColor(string htmlColor)
    {
        Color cl = new Color();
        ColorUtility.TryParseHtmlString(htmlColor, out cl);
        return cl;
    }

    public static void ChangePicture(Renderer rd, int pictureID)
    {
        Texture tx = (Texture) Resources.Load(string.Format("{0:000}", pictureID));
        if (tx == null) Debug.LogError("所选卡片没有图片资源：" + pictureID);
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        rd.GetPropertyBlock(mpb);
        mpb.SetTexture("_MainTex", tx);
        mpb.SetTexture("_EmissionMap", tx);
        rd.SetPropertyBlock(mpb);
    }

    public static IEnumerator MoveGameObject(Transform obj, Vector3 oldPosition, Quaternion oldRotation, Vector3 oldScale, Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale,float duration,float rotateDuration)
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
}