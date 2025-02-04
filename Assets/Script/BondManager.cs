using UnityEngine;
using Helper;
using System.Collections.Generic;
using System.IO;

public class BondManager : MonoBehaviour
{
    private Dictionary<(int, int), GameObject> bonds;
    private GameObject bondPrefab;
    public Transform parent;
    public float bondRadius = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bonds = new Dictionary<(int, int), GameObject>();
        bondPrefab = Resources.Load<GameObject>(Path.Combine("Prefab","Bond"));
    }
    public void UpdateBonds(Dictionary<int, Vector3> positions, HashSet<(int, int)> neighborPairs)
    {
        //TODO: Implement the object pooling pattern to reuse the bond objects
        
        // update bond position and check if the bond exists, if not create it
        foreach (var neighborPair in neighborPairs)
        {
            Vector3 topPosition = positions[neighborPair.Item1];
            Vector3 bottomPosition = positions[neighborPair.Item2];
            if(bonds.ContainsKey(neighborPair)){
                CreateBond(topPosition, bottomPosition, bonds[neighborPair]);
            }else{
                GameObject bondObject = Instantiate(bondPrefab);
                bondObject.transform.SetParent(parent);
                bondObject.transform.localScale = new Vector3(bondRadius, bondRadius, bondRadius);
                CreateBond(topPosition, bottomPosition, bondObject);
                bonds.Add(neighborPair, bondObject);
            }
        }
        // remove bonds that are not in the neighborPairs
        List<(int, int)> toRemove = new List<(int, int)>();
        foreach (var bond in bonds)
        {
            if(!neighborPairs.Contains(bond.Key)){
                Destroy(bond.Value);
                toRemove.Add(bond.Key);
            }
        }
        foreach (var key in toRemove)
        {
            bonds.Remove(key);
        }
        
    }

    private void CreateBond(Vector3 topPosition, Vector3 bottomPosition, GameObject bondObject)
    {
        if (bondObject == null)
        {
            Debug.LogError("Cylinder object is null! Provide a valid cylinder object.");
            return;
        }

        // Calculate the position of the cylinder (midpoint between top and bottom)
        Vector3 centerPosition = (topPosition + bottomPosition) / 2;

        // Calculate the height of the cylinder
        float height = Vector3.Distance(topPosition, bottomPosition);

        // Calculate the direction from bottom to top
        Vector3 direction = topPosition - bottomPosition;

        // Update the cylinder's position
        bondObject.transform.localPosition = centerPosition;

        // Update the cylinder's rotation
        // Adjust so the cylinder points along the direction (cylinder default Y-axis points up)
        bondObject.transform.localRotation = Quaternion.LookRotation(direction.normalized) * Quaternion.Euler(90f, 0f, 0f);

        // Update the cylinder's scale
        // Adjust the Y-axis scale to match the height (default Unity cylinder has a height of 2 units)
        Vector3 currentScale = bondObject.transform.localScale;
        bondObject.transform.localScale = new Vector3(currentScale.x, height / 2, currentScale.z);
    }
}
