using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * \brief Класс-конструктор элементов объемного визуализатора. 
 * \authors Пивко Артём
 * \version 1.0
 * \date 13.05.20
 * \warning Методы этого класса очень медленные и не предназначены для исполнения во время работы программы. Только в режиме редактирования. 
 *  
 * Скрипт занимается созданием и размещением объектов node на ребрах объемной сетки размера zoneSize и плотности density. \n
 * Рассчётное применение класса - создание элементов объемного визуализатора и задание им общего типа данных. 
 * 
 */
public class CubeCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject node = null;         ///< Размножаемый объект. Предпочтительно префаб. Задается из редактора.

    [SerializeField]
    private Vector3 zoneSize;               ///< Размер зоны по трём измерениям. Задается из редактора.
    [SerializeField]
    private Vector3 density;                ///< Плотность размещения объектов по трём измерениям. Задается из редактора.
    [SerializeField]
    private string dataType = null;         ///< Тип данных, которые будут слушать размещаемые элементы объемного визуализатора. 

    private GameObject[,,] nodes = null;    ///< Трёхмерная матрица, хранящая ссылки на созданные объекты. 
    private int xn, yn, zn = 0;             ///< Текущие размеры матрицы объектов. 

    /** \brief Метод создания объемной сетки объектов
     * Метод рассчитывает количество генерируемых объектов по каждой оси на основании размеров зоны и плотности. 
     * После этого вычисляется смещение offset, необходимое чтобы объекты находились внутри зоны, не пересекая её границ. 
     * Далее объекты инициируются на узлах 3D сетки и становятся по иерархии детьми данного объекта. 
     * Их названия меняются на Node[x,y,z], где x,y,z - его номер в объемной матрице. 
     * Если объект - элемент объемного визуализатора с компонентом NodeVisualisator, то его тип слушаемых данных устанавливается равным dataType.
     */
    public void GenerateCubes()
    {
        xn = (int)Mathf.Floor(zoneSize.x / density.x)+1;
        yn = (int)Mathf.Floor(zoneSize.y / density.y)+1;
        zn = (int)Mathf.Floor(zoneSize.z / density.z)+1;
        nodes = new GameObject[xn, yn, zn];

        float offset = node.transform.localScale.x / 2;
        Vector3 localoffset = this.transform.position + new Vector3(this.transform.localScale.x / 2, this.transform.localScale.y / 2, this.transform.localScale.y / 2);
        Vector3 currentCoord = new Vector3(offset, offset, offset);

        for (int i = 0; i < xn; i++)
        {
            for (int j = 0; j < yn; j++)
            {
                for(int k = 0; k < zn; k++)
                {
                    nodes[i, j, k] = Instantiate(node);
                    nodes[i, j, k].transform.SetParent(this.transform);
                    nodes[i, j, k].name = "Node[" + i.ToString() + "," + j.ToString() + "," + k.ToString() + "]";

                    nodes[i, j, k].transform.localPosition =  new Vector3(offset + i * density.x, offset + j * density.y, offset + k * density.z);
                    nodes[i, j, k].GetComponent<NodeVisualisator>().dataType = dataType;

                    //nodes[i, j, k].transform.position = currentCoord + localoffset;
                    //currentCoord += new Vector3(0, 0, density.z);
                }
                //currentCoord = new Vector3(offset + i * density.x, offset + j * density.y, offset);
            }
            //currentCoord = new Vector3(offset + i * density.x, offset, offset);
        }

    }

    /** \brief Метод уничтожения объемной сетки объектов
     * Метод уничтожает уже созданную в текущем выполнении программы сетку. 
     */
    public void DestroyCubes()
    {
        for (int i = 0; i < xn; i++)
        {
            for (int j = 0; j < yn; j++)
            {
                for (int k = 0; k < zn; k++)
                {
                    DestroyImmediate(nodes[i, j, k]);
                }
            }
        }
    }
    /** \brief Служебная функция, отображающая размеры границы в редакторе. */
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position+zoneSize/2, zoneSize);
    }
}
