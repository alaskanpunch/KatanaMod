using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

public class SwordItem : GrabbableObject
{
    public AudioSource swordAudio;
    private List<RaycastHit> objectsHitBySwordList = new List<RaycastHit>();
    public PlayerControllerB previousPlayerHeldBy;
    private RaycastHit[] objectsHitBySword;
    public int swordHitForce;
    public AudioClip[] hitSFX;
    public AudioClip[] swingSFX;
    private int swordMask = 11012424;
    private float timeAtLastDamageDealt;

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        RoundManager.PlayRandomClip(swordAudio, swingSFX);
        if (playerHeldBy != null)
        {
            previousPlayerHeldBy = playerHeldBy;
            if (playerHeldBy.IsOwner)
            {
                playerHeldBy.playerBodyAnimator.SetTrigger("UseHeldItem1");
            }
        }
        if (base.IsOwner)
        {
            HitSword();
        }
    }

    public override void PocketItem()
    {
        base.PocketItem();
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
    }

    public override void EquipItem()
    {
        base.EquipItem();
    }

    public void HitSword(bool cancel = false)
    {
        if (previousPlayerHeldBy == null)
        {
            Debug.LogError("Previousplayerheldby is null on this client when HitSword is called.");
            return;
        }
        previousPlayerHeldBy.activatingItem = false;
        bool flag = false;
        bool flag2 = false;
        int num = -1;
        if (!cancel)
        {
            previousPlayerHeldBy.twoHanded = false;
            objectsHitBySword = Physics.SphereCastAll(previousPlayerHeldBy.gameplayCamera.transform.position + previousPlayerHeldBy.gameplayCamera.transform.right * 0.1f, 0.8f, previousPlayerHeldBy.gameplayCamera.transform.forward, 1.4f, swordMask, QueryTriggerInteraction.Collide);
            objectsHitBySwordList = objectsHitBySword.OrderBy((RaycastHit x) => x.distance).ToList();
            for (int i = 0; i < objectsHitBySwordList.Count; i++)
            {
                if (objectsHitBySwordList[i].transform.gameObject.layer == 8 || objectsHitBySwordList[i].transform.gameObject.layer == 11)
                {
                    flag = true;
                    string text = objectsHitBySwordList[i].collider.gameObject.tag;
                    for (int j = 0; j < StartOfRound.Instance.footstepSurfaces.Length; j++)
                    {
                        if (StartOfRound.Instance.footstepSurfaces[j].surfaceTag == text)
                        {
                            num = j;
                            break;
                        }
                    }
                }
                else
                {
                    if (!objectsHitBySwordList[i].transform.TryGetComponent<IHittable>(out var component) || objectsHitBySwordList[i].transform == previousPlayerHeldBy.transform || (!(objectsHitBySwordList[i].point == Vector3.zero) && Physics.Linecast(previousPlayerHeldBy.gameplayCamera.transform.position, objectsHitBySwordList[i].point, out var _, StartOfRound.Instance.collidersAndRoomMaskAndDefault)))
                    {
                        continue;
                    }
                    flag = true;
                    Vector3 forward = previousPlayerHeldBy.gameplayCamera.transform.forward;
                    try
                    {
                        if (Time.realtimeSinceStartup - timeAtLastDamageDealt > 0.43f)
                        {
                            timeAtLastDamageDealt = Time.realtimeSinceStartup;
                            component.Hit(swordHitForce, forward, previousPlayerHeldBy, playHitSFX: true, 5);
                        }
                        flag2 = true;
                    }
                    catch (Exception arg)
                    {
                        Debug.Log($"Exception caught when hitting object with sword from player #{previousPlayerHeldBy.playerClientId}: {arg}");
                    }
                }
            }
        }
        if (flag)
        {
            RoundManager.PlayRandomClip(swordAudio, hitSFX);
            UnityEngine.Object.FindObjectOfType<RoundManager>().PlayAudibleNoise(base.transform.position, 17f, 0.8f);
            if (!flag2 && num != -1)
            {
                swordAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[num].hitSurfaceSFX);
                WalkieTalkie.TransmitOneShotAudio(swordAudio, StartOfRound.Instance.footstepSurfaces[num].hitSurfaceSFX);
            }
            HitSwordServerRpc(num);
        }
    }

    [ServerRpc]
    public void HitSwordServerRpc(int hitSurfaceID)
    {
        {
            HitSwordClientRpc(hitSurfaceID);
        }
    }
    [ClientRpc]
    public void HitSwordClientRpc(int hitSurfaceID)
    {
        if (!base.IsOwner)
        {
            RoundManager.PlayRandomClip(swordAudio, hitSFX);
            if (hitSurfaceID != -1)
            {
                HitSurfaceWithSword(hitSurfaceID);
            }
        }
    }
    private void HitSurfaceWithSword(int hitSurfaceID)
    {
        swordAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
        WalkieTalkie.TransmitOneShotAudio(swordAudio, StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
    }
}
