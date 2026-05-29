using UnityEngine;
using DG.Tweening;
using System;
using Cinemachine;

public class ShootingSystem
{
    private InputDataSo inputDataSo;
    private InkSystem inkSystem;
    private ParticleSystem particleSystem;
    private Transform gunTrans;
    private PlayerController playerController;
    private Transform splatGunNozzle;

    private bool wasShootInput=false;
    private CinemachineImpulseSource impulseSource;

    public ShootingSystem(InputDataSo inputDataSo, InkSystem inkSystem, ParticleSystem particleSystem,
        PlayerController playerController,Transform splatGunNozzle,Transform gunTrans)
    {
        this.inputDataSo = inputDataSo;
        this.inkSystem = inkSystem;
        this.particleSystem = particleSystem;
        this.gunTrans = gunTrans;
        this.playerController = playerController;
        this.splatGunNozzle = splatGunNozzle;
        impulseSource = this.playerController.GetComponent<CinemachineImpulseSource>();
    }

    public void Update()
    {
        if (inputDataSo == null)
        {
            Debug.Log("输入SO为空");
            return;
        }

        if (inputDataSo.shootInput)
        {
            VisualPolish();
        }

        if (inputDataSo.shootInput&&wasShootInput!=inputDataSo.shootInput)
        {
            wasShootInput = true;
            Shoot();
            playerController.RotateToCamera();
        }
        else if (!inputDataSo.shootInput&&wasShootInput!=inputDataSo.shootInput)
        {
            wasShootInput = false;
            StopShooting();
        }
    }

    private void Shoot()
    {
        particleSystem.Play();
    }
    private void StopShooting()
    {
        particleSystem.Stop();
    }

    private void VisualPolish() 
    {
        if (!DOTween.IsTweening(gunTrans))
        {
            gunTrans.DOComplete();

            Vector3 localPosition=gunTrans.localPosition;

            gunTrans.DOLocalMove(localPosition - new Vector3(0, 0, .2f), .03f)
                .OnComplete(() => gunTrans.DOLocalMove(localPosition, .1f).SetEase(Ease.OutSine));

            impulseSource.GenerateImpulse();
        }

        if (!DOTween.IsTweening(splatGunNozzle))
        {
            splatGunNozzle.DOComplete();
            splatGunNozzle.DOPunchScale(new Vector3(0, 1, 1) / 1.5f, .15f, 10, 1);
        }
    }
}
