using UnityEngine;
using DG.Tweening;
using System;
using Cinemachine;

public class ShootingSystem
{
    private InputDataSo inputDataSo;
    private InkSystem inkSystem;
    private ParticleSystem particleSystem;
    private Transform playerControllerTrans;
    private Transform splatGunNozzle;

    private bool wasShootInput=false;
    private CinemachineImpulseSource impulseSource;

    public ShootingSystem(InputDataSo inputDataSo, InkSystem inkSystem, ParticleSystem particleSystem,
        Transform playerController,Transform splatGunNozzle)
    {
        this.inputDataSo = inputDataSo;
        this.inkSystem = inkSystem;
        this.particleSystem = particleSystem;
        this.playerControllerTrans = playerController;
        this.splatGunNozzle = splatGunNozzle;
        impulseSource = playerControllerTrans.GetComponent<CinemachineImpulseSource>();
    }

    public void Update()
    {
        if (inputDataSo.shootInput&&wasShootInput!=inputDataSo.shootInput)
        {
            wasShootInput = true;
            Shoot();
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
        if (!DOTween.IsTweening(playerControllerTrans))
        {
            playerControllerTrans.DOComplete();

            Vector3 localPosition=playerControllerTrans.localPosition;

            playerControllerTrans.DOLocalMove(localPosition - new Vector3(0, 0, .2f), .03f)
                .OnComplete(() => playerControllerTrans.DOLocalMove(localPosition, .1f).SetEase(Ease.OutSine));

            impulseSource.GenerateImpulse();
        }

        if (!DOTween.IsTweening(splatGunNozzle))
        {
            splatGunNozzle.DOComplete();
            splatGunNozzle.DOPunchScale(new Vector3(0, 1, 1) / 1.5f, .15f, 10, 1);
        }
    }
}
