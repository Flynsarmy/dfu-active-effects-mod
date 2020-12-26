using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System.Collections.Generic;

namespace FlynsarmyActiveEffectsMod
{
    public class ActiveEffects : MonoBehaviour
    {
        //Options
        private Color fontColor = new Color(0.85f, 0.85f, 0.85f);
        
        //External Data handlers
        DaggerfallUI daggerfallUI;
        PlayerEntity player;

        private string message;

        //Internal 
        private float UIScale;
        private GUIStyle guiStyle = new GUIStyle();
        
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            GameObject go = new GameObject(initParams.ModTitle);
            go.AddComponent<ActiveEffects>();

            ModManager.Instance.GetMod(initParams.ModTitle).IsReady = true;

            Debug.Log("ActiveEffects: Init");
        }

        private void Start()
        {
            guiStyle.normal.textColor = fontColor;
            daggerfallUI = GameObject.Find("DaggerfallUI").GetComponent<DaggerfallUI>();
            guiStyle.alignment = TextAnchor.UpperLeft;

            player = GameManager.Instance.PlayerEntity;

            // Update the message each magic round
            EntityEffectBroker.OnNewMagicRound += FlynsarmyActiveEffects_OnNewMagicRound;

            Debug.Log("ActiveEffects: Starto!");
        }

        private void LateUpdate()
        {
            UIScale = daggerfallUI.DaggerfallHUD.HUDVitals.Scale.x;
            guiStyle.fontSize = (int)(5.0f * UIScale);
        }

        private void FlynsarmyActiveEffects_OnNewMagicRound()
        {
            message = GetActiveEffectsMessage();
        }

        private void OnGUI()
        {
            if (daggerfallUI.UserInterfaceManager.TopWindow != daggerfallUI.DaggerfallHUD || GameManager.IsGamePaused)
            {
                return;
            }

            GUI.color = Color.white;
            GUI.depth = 0; //To ensure that the graphics are displayed above everything else (else the player weapon will render above it)

            GUI.Label(
                new Rect(
                    (5 * UIScale),
                    (2 * UIScale),
                    32 * UIScale,
                    16 * UIScale
                ),
                message,
                guiStyle
            );
        }

        // Taken from DaggerfallWorkshop.Game.DaggerfallUI::CreateHealthStatusBox()
        private string GetActiveEffectsMessage()
        {
            // Show "You are healthy." if there are no diseases and no poisons
            int diseaseCount = GameManager.Instance.PlayerEffectManager.DiseaseCount;
            int poisonCount = GameManager.Instance.PlayerEffectManager.PoisonCount;
            if (diseaseCount > 0 || poisonCount > 0)
            {
                //Debug.Log(string.Format("ActiveEffects: {0} diseases, {1} poisons", diseaseCount, poisonCount));

                List<string> messages = new List<string>();
                EntityEffectManager playerEffectManager = GameManager.Instance.PlayerEffectManager;

                LiveEffectBundle[] bundles = playerEffectManager.EffectBundles;
                foreach (LiveEffectBundle bundle in bundles)
                {
                    foreach (IEntityEffect effect in bundle.liveEffects)
                    {
                        if (effect is DiseaseEffect)
                        {
                            string diseaseType;
                            DiseaseEffect disease = (DiseaseEffect)effect;

                            if (effect is LycanthropyInfection)
                            {
                                diseaseType = "Lycanthropy";
                            }
                            else if (effect is VampirismInfection)
                            {
                                diseaseType = "Vampirism";
                            }
                            else
                            {
                                diseaseType = ((Diseases)((int)disease.ClassicDiseaseType)).ToString();
                            }

                            if (disease.IncubationOver)
                            {
                                messages.Add(
                                    string.Format(
                                        "You have contracted {0}",
                                        diseaseType
                                    )
                                );
                            }
                            else
                            {
                                messages.Add(
                                    string.Format(
                                        "{0} is slowly creeping over you",
                                        diseaseType
                                    )
                                );
                            }
                        }
                        else if (effect is PoisonEffect)
                        {
                            PoisonEffect poison = (PoisonEffect)effect;
                            Poisons poisonType = (Poisons)poison.CurrentVariant + 128;
                            if (poison.CurrentState != PoisonEffect.PoisonStates.Waiting)
                            {
                                messages.Add(
                                    string.Format(
                                        "You have {0} coursing through your veins",
                                        poisonType.ToString()
                                    )
                                );
                            }
                            else
                            {
                                messages.Add(
                                    string.Format(
                                        "{0} is slowly seeping through your veins",
                                        poisonType.ToString()
                                    )
                                );
                            }
                        }
                    }
                }

                if (messages.Count > 0)
                {
                    return string.Join("\n", messages.ToArray());
                }
            }

            return "";
        }
    }
}
