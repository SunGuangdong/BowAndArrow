using UnityEngine;
using System.Collections;


/// <summary>
/// 它实现了向上漂移分数显示
/// </summary>
[RequireComponent(typeof(TextMesh))]
public class RisingText : MonoBehaviour
{
	// 私有变量
	Vector3 crds_delta;
	float   alpha;
	float   life_loss;
	Camera  cam;
	
	// 设置分数的颜色
	public Color color = Color.white;
	
	// SETUP -可以在  start 函数中调用！
	// "points" 显示的分数
	// "duration" 持续时间
	// "rise speed" 快速移动
	public void setup(int points, float duration, float rise_speed)
	{
		GetComponent<TextMesh>().text = points.ToString();        
		life_loss = 1f / duration;
		crds_delta = new Vector3(0f, rise_speed, 0f);        
	}
	
	void Start()
	{
		alpha = 1f;
		cam = GameObject.Find("Main Camera").GetComponent<Camera>();
		crds_delta = new Vector3(0f, 1f, 0f);
		life_loss = 0.5f;
	}
	
	void Update () 
	{
		// 向上移动
		transform.Translate(crds_delta * Time.deltaTime, Space.World);
		
		// 同时渐变
		alpha -= Time.deltaTime * life_loss;
		GetComponent<Renderer>().material.color = 
            new Color(color.r,color.g,color.b,alpha);
		
		// 结束了 销毁
		if (alpha <= 0f)
            Destroy(gameObject);
		
		// 分数要面向摄像机
		transform.LookAt(cam.transform.position);
		transform.rotation = cam.transform.rotation;        
	}
}