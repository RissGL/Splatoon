using Cinemachine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;
using ZGameFrameWork.Core;
using System.Collections;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private GameObject humanModel;
    [SerializeField] private GameObject squidModel;
    [Label("潜水粒子")]
    [SerializeField]private ParticleSystem squidDivePar;


    private PlayerController playerController;


    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        squidDivePar.Stop();
    }

    private void Start()
    {
        EventCenter.AddEventListener<bool>((int)EventID.OnSquidDiveChange, ChangeSquidModel);
    }
    private void OnDisable()
    {
        EventCenter.RemoveEventListener<bool>((int)EventID.OnSquidDiveChange, ChangeSquidModel);
    }

    /// <summary>
    /// 切换潜水模型和粒子显示状态
    /// </summary>
    /// <param name="t"></param>
    private void ChangeSquidModel(bool t)
    {
        squidModel.SetActive(t);
        if (t)
        {
            squidDivePar.Stop();
        }
        else
        {
            squidDivePar.Play();
        }

        StartCoroutine(CheckSquidModel());
    }

    private IEnumerator CheckSquidModel()
    {
        yield return null;
        if (playerController.GetCurrentState() == PlayerMovementState.HumanRun ||
    playerController.GetCurrentState() == PlayerMovementState.HumanAir)
        {
            squidModel.SetActive(false);
            squidDivePar.Stop();
        }
    }
}