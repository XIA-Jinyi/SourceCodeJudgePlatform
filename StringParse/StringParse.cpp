/******************************************************************************
* 
* [FileName]    StringParse.cpp
* 
* [Author]      刘苏锐
* 
* [CreateDate]  2022/04/27
* 
******************************************************************************/

#include<iostream>
#include<string>
#include<string.h>
using namespace std;
int main() {
    int times;
    bool isError = false;
    string str;         //被替换语句
    string mainSubStr;  //替换语句（包括替换部分和被替换部分）
    string srcSubStr;   //储存被替换部分
    string dstSubStr;   //储存替换部分
    getline(cin, str);  //相关输入
    cin >> times;
    int lf = getchar();          //去除\n防止影响getline
    for (int b = 0; b < times; b++) {
        getline(cin, mainSubStr);
        int  b1 = -1, b2 = 0, c1 = 0, c2 = 0;
        for (int n = 0; n < mainSubStr.length(); n++) {         //获取“<”“>”的位置
            if (mainSubStr[n] == '<') {
                b1 = n;
            }
            else if (mainSubStr[n] == '>') {
                b2 = n + 1;
                break;
            }
        }
        if (b1 == -1 || b2 == 0) {
            continue;
        }
        for (int n = b2 - 1; n < mainSubStr.length(); n++) {    //获取“"”的位置
            if (mainSubStr[n] == '"') {
                c1 = n;
                break;
            }

        }
        c2 = mainSubStr.length() - c1;
        srcSubStr = mainSubStr.substr(b1, b2);  //通过substr函数以及相关位置得到想要的部分
        dstSubStr = mainSubStr.substr(c1, c2);
        int pos;                                //检验是否可替换，不可以pos返回-1，可以就返回相关位置
        pos = str.find(srcSubStr);
        if (pos == -1) {                        //不可替换，结束并返回1
            cerr << srcSubStr << " not found." << endl;
            isError = true;
        }
        while (pos != -1)                       //可替换，通过replace函数进行替换
        {
            str.replace(pos, srcSubStr.length(), dstSubStr);
            pos = str.find(srcSubStr);
        }
    }
    if (isError) {
        return 1;
    }
    cout << str << endl;    //输出
    //system("pause");
    return 0;
}
