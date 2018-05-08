using System.Collections;
using System.Collections.Generic;
using Riverlake.RoleEntity;
using UnityEngine;

public class ArtCharactorTest : MonoBehaviour
{
    [Range(0.01f , 1 )]
    public float Speed = 1;

    public string skeleton;
    public string[] widgets;
    public string[] weapons;
    

    private CharacterPlayerEntity entity;
    private string prefabRoot = "RoleModels/Players";

    private static int listCount = 3;

    private bool[] weapon_list ;
    private bool[] head_list ;
    private bool[] face_list;
    private bool[] chest_list ;
    private bool[] hand_list ;
    private bool[] feet_list ;
    private bool[] wing_list ;

    /// <summary>
    /// Config default equipment informations.
    /// </summary>
    private const int DEFAULT_WEAPON = 0;
    private const int DEFAULT_HEAD = 0;
    private const int DEFAULT_FACE = 0;
    private const int DEFAULT_CHEST = 0;
    private const int DEFAULT_HAND = 0;
    private const int DEFAULT_FEET = 0;
    private const int DEFAULT_WING = 1;

    private List<string> anims = new List<string>(); 
    // Use this for initialization
    void Start ()
    {
        listCount = Mathf.Clamp(widgets.Length, 1, 3);
        weapon_list = new bool[listCount];
        head_list = new bool[listCount];
        face_list = new bool[listCount];
        chest_list = new bool[listCount];
        hand_list = new bool[listCount];
        feet_list = new bool[listCount];
        wing_list = new bool[listCount];

        CharacterPlayerEntity playerEntity = new CharacterPlayerEntity();
        entity = playerEntity;
        playerEntity.InitEntity(skeleton,
            string.Format("{0}/{1}/{1}_head", prefabRoot, widgets[DEFAULT_HEAD]),
            string.Format("{0}/{1}/{1}_face", prefabRoot, widgets[DEFAULT_FACE]),
            string.Format("{0}/{1}/{1}_chest", prefabRoot, widgets[DEFAULT_CHEST]),
            string.Format("{0}/{1}/{1}_hand", prefabRoot, widgets[DEFAULT_HAND]),
            string.Format("{0}/{1}/{1}_feet", prefabRoot, widgets[DEFAULT_FEET]),
            weapons == null ? "" : "RoleModels/Weapons/" + weapons[DEFAULT_WEAPON] + "/" + weapons[DEFAULT_WEAPON]); //
        playerEntity.MainTransform.localPosition = new Vector3(0.85f , -1.6f, -4.79f);
        playerEntity.MainTransform.localRotation = Quaternion.Euler(0, 180, 0);
        //            playerEntity.OnLoad();
        playerEntity.OnLoadFinish = onLoadFinish;
    }

    private void onLoadFinish(ACharacterEntity acterEntity)
    {
        anims.Clear();
        Animation acterAnim = acterEntity.MainTransform.Find("Skeleton").GetComponentInChildren<Animation>();
        foreach (AnimationState animState in acterAnim)
        {
            animState.speed = Speed;
            anims.Add(animState.name);
        }
        Debug.Log("Animation count:" + anims.Count + " , clip:" + acterAnim.GetClipCount());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical();
            onLeftGUI();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            onRightGUI();
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(180);

        this.onBottomGUI();

        GUILayout.Space(10);
    }

    private void onLeftGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Euipments 1", GUILayout.Width(100), GUILayout.Height(20));
        GUILayout.Label("Euipments 2", GUILayout.Width(100), GUILayout.Height(20));
        GUILayout.EndHorizontal();

        this.OnGUIItem(weapon_list, "Weapon");

        this.OnGUIItem(head_list, "Head");

        this.OnGUIItem(chest_list, "Chest");

        this.OnGUIItem(hand_list, "Hand");

        this.OnGUIItem(feet_list, "Feet");

    }


    private void OnGUIItem(bool[] list, string title)
    {
        GUILayout.BeginHorizontal();
        for (int i = 0; i < list.Length; i++)
        {

            if (GUILayout.Button(title + (list[i] ? "(√)" : ""), GUILayout.Width(100), GUILayout.Height(30)))
            {
                if (!list[i])
                {
                    for (int j = 0; j < list.Length; j++)
                    {
                        list[j] = false;
                    }
                    list[i] = true;

                    if (title == "Head")
                    {
                        entity.Header = string.Format("{0}/{1}/{1}_head" , prefabRoot , widgets[i]);
                    }
                    else if (title == "Chest")
                    {
                        entity.Body = string.Format("{0}/{1}/{1}_chest" , prefabRoot , widgets[i]);
                    }
                    else if (title == "Hand")
                    {
                        entity.Hand = string.Format("{0}/{1}/{1}_hand" , prefabRoot , widgets[i]);
                    }
                    else if (title == "Feet")
                    {
                        entity.Feet = string.Format("{0}/{1}/{1}_feet" , prefabRoot , widgets[i]);
                    }
                    else if (title == "Weapon" && weapons != null && i < weapons.Length)
                    {
                        entity.Weapon = string.Format("RoleModels/Weapons/{0}/{0}" , weapons[i]);
                    }
                    entity.OnLoad();
                }
            }
        }
        GUILayout.EndHorizontal();
    }


    private void onRightGUI()
    {
//        if (GUILayout.Button("Load Charactor", GUILayout.Height(30)))
//        {
//            CharacterPlayerEntity playerEntity = new CharacterPlayerEntity();
//            entity = playerEntity;
//            playerEntity.InitEntity(skeleton, head, face, chest, hand, feet, weapon); //
//            playerEntity.MainTransform.localPosition = Vector3.one;
//            playerEntity.MainTransform.localRotation = Quaternion.Euler(0, 180, 0);//            playerEntity.OnLoad();
//        }
    }


    private void onBottomGUI()
    {
        if (anims.Count <= 0) return;

        GUILayout.BeginHorizontal();
        GUILayout.Space(80);
        GUILayout.BeginVertical();
        GUILayoutOption width = GUILayout.Width(100);
        int i = 0;
        for (i = 0; i < anims.Count; )
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < 7; j++)
            {
                if (GUILayout.Button(anims[i], width))
                {
                    entity.Play(anims[i]);
                }
                i++;

                if (i >= anims.Count) break;

            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        GUILayout.Space(80);
        GUILayout.EndHorizontal();
    }
}
