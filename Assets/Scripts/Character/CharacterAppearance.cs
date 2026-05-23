using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class CharacterAppearance 
{
    private GameObject humanModelInstance;
    private GameObject squidModelInstance;

    private Transform modelContainer;
    public Animator humanAnimator { get; private set; }
    public Animator squidAnimator { get; private set; }

    public CharacterAppearance(Transform modelContainer,PlayerConfig playerConfig,
        GameObject humanModel,GameObject squidModel)
    {
        this.modelContainer = modelContainer;

        //humanModelInstance = GameObject.Instantiate(playerConfig.humanAppearance.appearanceModel, modelContainer);
        //squidModelInstance = GameObject.Instantiate(playerConfig.squidAppearance.appearanceModel, modelContainer);


        this.humanModelInstance = humanModel;
        this.squidModelInstance = squidModel;

        humanModelInstance.SetActive(true);
        squidModelInstance.SetActive(false);

        humanAnimator = humanModelInstance.GetComponent<Animator>();
        if (humanAnimator == null)
        {
            humanAnimator = humanModelInstance.AddComponent<Animator>();
            Debug.LogWarning("人类预制体缺失animator");
        }

        squidAnimator = squidModelInstance.GetComponent<Animator>();
        if (squidAnimator == null)
        {
            squidAnimator = squidModelInstance.AddComponent<Animator>();
            Debug.LogWarning("乌贼预制体缺失animator");
        }

        humanAnimator.runtimeAnimatorController = playerConfig.humanAppearance.animatorController;
        squidAnimator.runtimeAnimatorController = playerConfig.squidAppearance.animatorController;
    }

    public void SwitchToSquid() 
    {
        humanModelInstance?.SetActive(false);
        squidModelInstance?.SetActive(true);
    }

    public void SwitchToHuman() 
    {
        humanModelInstance?.SetActive(true);
        squidModelInstance?.SetActive(false);
    }
}
