using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    Transform globalInfo;

    Transform entityInfo;
    Transform lifebar;

    Transform factoryBuilds;
    Transform factoryBuildsEntityInfo;
    Transform factoryBuildsEntityInfoBuildInfo;
    Transform factoryBuildsEntityInfoBuildInfoEnergyCost;
    Transform factoryBuildsEntityInfoBuildInfoBuildTime;
    Transform factoryBuildsBuildList;

    Transform engineerBuilds;
    Transform engineerBuildsEntityInfo;
    Transform engineerBuildsEntityInfoBuildInfo;
    Transform engineerBuildsEntityInfoBuildInfoEnergyCost;
    Transform engineerBuildsEntityInfoBuildInfoBuildTime;

    // Start is called before the first frame update
    void Start()
    {
        globalInfo = transform.Find("GlobalInfo");

        entityInfo = transform.Find("EntityInfo");
        lifebar = entityInfo.Find("Lifebar");

        factoryBuilds = transform.Find("FactoryBuilds");
        factoryBuildsEntityInfo = factoryBuilds.Find("EntityInfo");
        factoryBuildsEntityInfoBuildInfo = factoryBuildsEntityInfo.Find("BuildInfo");
        factoryBuildsEntityInfoBuildInfoEnergyCost = factoryBuildsEntityInfoBuildInfo.Find("EnergyCost");
        factoryBuildsEntityInfoBuildInfoBuildTime = factoryBuildsEntityInfoBuildInfo.Find("BuildTime");
        factoryBuildsBuildList = factoryBuilds.Find("BuildList");

        engineerBuilds = transform.Find("EngineerBuilds");
        engineerBuildsEntityInfo = engineerBuilds.Find("EntityInfo");
        engineerBuildsEntityInfoBuildInfo = engineerBuildsEntityInfo.Find("BuildInfo");
        engineerBuildsEntityInfoBuildInfoEnergyCost = engineerBuildsEntityInfoBuildInfo.Find("EnergyCost");
        engineerBuildsEntityInfoBuildInfoBuildTime = engineerBuildsEntityInfoBuildInfo.Find("BuildTime");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateEngineerBuildsEntityInfo(Entity entity)
    {
        engineerBuildsEntityInfo.Find("EntityIcon").GetComponent<Image>().sprite = entity.icon;
        engineerBuildsEntityInfo.Find("EntityName").GetComponent<Text>().text = entity.name;
        engineerBuildsEntityInfo.Find("EntityDescription").GetComponent<Text>().text = entity.description;

        engineerBuildsEntityInfoBuildInfoEnergyCost.Find("EnergyText").GetComponent<Text>().text = entity.energyCost.ToString();

        int timeMinute = Mathf.FloorToInt(entity.buildTime / 60f);
        int timeSecond = Mathf.FloorToInt(entity.buildTime) % 60;

        string timeStr = timeMinute.ToString().PadLeft(2, '0') + ":" + timeSecond.ToString().PadLeft(2, '0');

        engineerBuildsEntityInfoBuildInfoBuildTime.Find("TimeText").GetComponent<Text>().text = timeStr;
    }

    public void EnableEngineerBuildsEntityInfo(bool enable)
    {
        engineerBuildsEntityInfo.gameObject.SetActive(enable);
    }

    public void EnableEngineerBuilds(bool enable)
    {
        engineerBuilds.gameObject.SetActive(enable);
    }

    public void UpdateFactoryBuildsBuildList(Factory factory)
    {
        for (int i = 0; i < 12; i++) // UI can display only 12 builds icons
        {
            Transform buildIcon = factoryBuildsBuildList.Find("Build " + i.ToString());

            if (i >= factory.buildList.Count)
            {
                buildIcon.gameObject.SetActive(false);
                continue;
            }

            buildIcon.gameObject.SetActive(true);

            Entity entity = factory.buildList[i];

            buildIcon.GetComponent<Image>().sprite = entity.icon;
        }
    }

    public void EnableFactoryBuildsEntityInfo(bool enable)
    {
        factoryBuildsEntityInfo.gameObject.SetActive(enable);
    }

    public void UpdateFactoryBuildsEntityInfo(Entity entity)
    {
        factoryBuildsEntityInfo.Find("EntityIcon").GetComponent<Image>().sprite = entity.icon;
        factoryBuildsEntityInfo.Find("EntityName").GetComponent<Text>().text = entity.name;
        factoryBuildsEntityInfo.Find("EntityDescription").GetComponent<Text>().text = entity.description;

        factoryBuildsEntityInfoBuildInfoEnergyCost.Find("EnergyText").GetComponent<Text>().text = entity.energyCost.ToString();

        int timeMinute = Mathf.FloorToInt(entity.buildTime / 60f);
        int timeSecond = Mathf.FloorToInt(entity.buildTime) % 60;

        string timeStr = timeMinute.ToString().PadLeft(2, '0') + ":" + timeSecond.ToString().PadLeft(2, '0');

        factoryBuildsEntityInfoBuildInfoBuildTime.Find("TimeText").GetComponent<Text>().text = timeStr;
    }

    public void EnableFactoryBuilds(bool enable)
    {
        factoryBuilds.gameObject.SetActive(enable);
    }

    public void EnableEnetityInfo(bool enable)
    {
        entityInfo.gameObject.SetActive(enable);
    }

    public void UpdateEnetityInfo(Entity entity)
    {
        entityInfo.Find("EntityIcon").GetComponent<Image>().sprite = entity.icon;
        entityInfo.Find("EntityName").GetComponent<Text>().text = entity.name;
        entityInfo.Find("EntityDescription").GetComponent<Text>().text = entity.description;

        lifebar.GetComponent<Slider>().value = entity.hp / (float)entity.hpMax;
        lifebar.Find("LifebarText").GetComponent<Text>().text = entity.hp.ToString() + " / " + entity.hpMax.ToString();
    }

    public void UpdateGlobalInfo(Player player)
    {
        globalInfo.Find("EnergyText").GetComponent<Text>().text = player.energy.ToString();
    }

    static public UI Get() // ?
    {
        return GameObject.Find("UI").GetComponent<UI>();
    }
}
