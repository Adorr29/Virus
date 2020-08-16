using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

enum SelectionType
{
    None,
    Engineer,
    OffensiveUnit,
    Factory,
    Enemy // ??
};

public class Player : MonoBehaviour
{
    SelectionType selectionType = SelectionType.None;
    List<Entity> selectedEntitys = new List<Entity>();

    Entity selectedBuild = null;
    Entity previewBuild;

    public int energy = 100;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UI.Get().UpdateGlobalInfo(this);

        if (selectionType == SelectionType.Factory)
        {
            UI.Get().EnableFactoryBuilds(true);
            UI.Get().UpdateFactoryBuildsBuildList(selectedEntitys.First() as Factory);
        }
        else
            UI.Get().EnableFactoryBuilds(false);

        if (selectionType == SelectionType.Engineer)
        {
            UI.Get().EnableEngineerBuilds(true);
            // TODO call a func of UI
        }
        else
            UI.Get().EnableEngineerBuilds(false);

        // check if mouse is on UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            UI.Get().EnableEnetityInfo(false);
            return;
        }

        GameObject select = GetMouseSelection();

        if (select != null)
        {
            Entity entity = select.GetComponent<Entity>();

            UI.Get().EnableEnetityInfo(entity != null);

            if (entity != null)
                UI.Get().UpdateEnetityInfo(entity);
        }

        if (selectedBuild)
        {
            if (Input.GetKeyDown(KeyCode.R)) // TODO use Input settings
                previewBuild.transform.Rotate(new Vector3(0, 90, 0));

            if (select != null)
            {
                Tile tile = select.GetComponent<Tile>();

                if (tile != null)
                {
                    previewBuild.transform.position = tile.transform.Find("EntityPosition").position;

                    GameManager.Get().ValidPreviewPosition(GameManager.Get().IsWalkable(tile));
                }
                else
                {
                    Entity entity = select.GetComponent<Entity>();

                    if (entity != null)
                    {
                        previewBuild.transform.position = entity.transform.position;

                        GameManager.Get().ValidPreviewPosition(false);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape)) // TODO use Input settings
                ClearSelectedBuild();
        }

        if (Input.GetMouseButtonDown(0)) // 0 is left click
        {
            Tile tile = select?.GetComponent<Tile>();

            if (selectionType == SelectionType.Engineer && selectedBuild != null && tile != null && GameManager.Get().IsWalkable(tile))
            {
                SoundManager.Get().PlaySound("BuildOrder");

                foreach (Entity entity in selectedEntitys)
                    (entity as Engineer)?.StartFoundation(selectedBuild, tile.position, previewBuild.transform.rotation.eulerAngles.y);
            }
            else
            {
                // if player click select = new selection, if player shift click new selection added to select
                if (!Input.GetKey(KeyCode.LeftShift)) // TODO use Input settings
                    ClearSelection();

                //GameObject select = GetMouseSelection();

                if (select != null)
                    if (!ManageEntitySelect<Engineer>(select, SelectionType.Engineer))
                        if (!ManageEntitySelect<OffensiveUnit>(select, SelectionType.OffensiveUnit))
                            if (!ManageEntitySelect<Factory>(select, SelectionType.Factory))
                                //if (!ManageEntitySelect<Virus>(select, SelectionType.Enemy)) // ??
                                Debug.Log(select);
            }

            ClearSelectedBuild();
        }
        else if (Input.GetMouseButtonDown(1)) // 1 is right click
        {
            //GameObject select = GetMouseSelection();

            if (select != null)
            {
                if (selectionType == SelectionType.Engineer)
                {
                    Tile tile = select.GetComponent<Tile>();

                    if (tile != null)
                    {
                        ClearSelectedBuild();

                        if (GameManager.Get().IsWalkable(tile))
                        {
                            SoundManager.Get().PlaySound("MoveOrder");

                            foreach (Entity entity in selectedEntitys)
                            {
                                entity?.StartMove(tile.position);
                                (entity as Engineer)?.Cancel();
                            }
                        }
                    }
                    else
                    {
                        Entity selectEntity = select.GetComponent<Entity>();

                        if (selectEntity.buildProgress < 1f)
                        {
                            SoundManager.Get().PlaySound("BuildOrder");

                            foreach (Entity entity in selectedEntitys)
                                (entity as Engineer)?.StartBuild(selectEntity);
                        }
                        else if (!(selectEntity is Virus))
                        {
                            SoundManager.Get().PlaySound("BuildOrder"); // ?

                            foreach (Entity entity in selectedEntitys)
                                (entity as Engineer)?.StartRepair(selectEntity);
                        }
                    }
                }
                else if (selectionType == SelectionType.OffensiveUnit)
                {
                    // currently Virus is the only enemy of the game
                    Virus virus = select.GetComponent<Virus>();

                    if (virus != null)
                    {
                        SoundManager.Get().PlaySound("AttackOrder");

                        foreach (Entity entity in selectedEntitys)
                            (entity as OffensiveUnit)?.StartAttack(virus);
                    }
                    else
                    {
                        Tile tile = select.GetComponent<Tile>();

                        if (tile != null && GameManager.Get().IsWalkable(tile))
                        {
                            SoundManager.Get().PlaySound("MoveOrder");

                            foreach (Entity entity in selectedEntitys)
                                if (entity != null)
                                {
                                    OffensiveUnit unit = entity as OffensiveUnit;

                                    unit.StopAttack();
                                    unit.StartMove(tile.position);
                                }
                        }
                    }
                }
                else if (selectionType == SelectionType.Factory)
                {
                    Tile tile = select.GetComponent<Tile>();

                    if (tile != null && GameManager.Get().IsWalkable(tile))
                    {
                        SoundManager.Get().PlaySound("MoveOrder");

                        foreach (Entity entity in selectedEntitys)
                            if (entity != null)
                                (entity as Factory).moveAfterBuild = tile.position;
                    }
                }
            }
        }
    }

    public void SetSelectedBuild(Entity entity)
    {
        selectedBuild = entity;

        if (previewBuild != null)
            Destroy(previewBuild.gameObject);

        previewBuild = Instantiate(entity);

        GameManager.Get().ToPreview(previewBuild.gameObject);

        // tmp position
        Vector2Int worldSize = GameManager.Get().world.size;
        previewBuild.transform.position = new Vector3(worldSize.x / 2, -5, worldSize.y / 2);
    }

    public void ClearSelectedBuild()
    {
        selectedBuild = null;

        if (previewBuild != null)
        {
            Destroy(previewBuild.gameObject);

            previewBuild = null;
        }
    }

    public void AddToFactoryBuildList(Unit unit)
    {
        if (selectionType != SelectionType.Factory)
            return;

        foreach (Entity entity in selectedEntitys)
            (entity as Factory)?.AddToBuildList(unit);
    }

    bool ManageEntitySelect<T>(GameObject select, SelectionType newSelectionType) where T : Entity
    {
        T entity = select.GetComponent<T>();

        if (entity == null)
            return false;

        ChangeSelectionType(newSelectionType);

        if (selectedEntitys.Contains(entity))
            RemoveToSelection(entity);
        else
            AddToSelection(entity);

        return true;
    }

    void AddToSelection(Entity entity)
    {
        SoundManager.Get().PlaySound("AddToSelection");

        selectedEntitys.Add(entity);
        entity.Select();
    }

    public void RemoveToSelection(Entity entity)
    {
        if (selectedEntitys.Remove(entity))
        {
            SoundManager.Get().PlaySound("RemoveToSelection");

            entity.Deselect();

            if (selectedEntitys.Count == 0)
                selectionType = SelectionType.None;
        }
    }

    bool ChangeSelectionType(SelectionType newSelectionType)
    {
        if (selectionType == newSelectionType)
            return false;

        ClearSelection();
        selectionType = newSelectionType;

        if (selectionType != SelectionType.Engineer)
            ClearSelectedBuild();

        return true;
    }

    void ClearSelection()
    {
        foreach (Entity entity in selectedEntitys)
            entity?.Deselect();
        selectedEntitys.Clear();
        selectionType = SelectionType.None;
    }

    public GameObject GetMouseSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            GameObject select = hit.collider.gameObject;

            return GameManager.Get().AscendToTheBaseObject(select);
        }
        return null;
    }

    static public Player Get() // ?
    {
        return GameObject.Find("Player").GetComponent<Player>();
    }
}
