/********************************************************************
	created:	2014/12/08
	filename: 	tpug\tpcts\f413076.cpp
	file path:	tpug\tpcts
	file base:	f413076
	file ext:	cpp
	author:		ZSJ
	
	purpose:	查询快订历史资金
	in:
	sname	投资者子账号对外账号	CHAR	20	char[21]	ClientID
	sdate0	日期	CHAR	8	char[9]	yyyymmdd必传

	
	out:
	sname	投资者子账号对外账号	CHAR	20	char[21]
	sbank_code2	币种	CHAR	6	char[7]
	sdate0	日期	CHAR	8	char[9]
	damt0	清算后余额	DECIMAL	18,4	double

		
	retCode	返回代码	INTEGER	4	unsigned int 
	vsmess	返回信息	VARCHAR	255	char[256]
					  
*********************************************************************/
#include "stdafx.h"
#include "mypub.h"
#include "cpack.h"
#include "bupub.h"
#include "ugdata.h"
#include "smtbase.h"
#include "tableclass.h"
#include "tpctsfuncs.h"
#include "KSCTS.h"

extern CSvrLink *g_pSvrLink;  // 与业务调度中心的连接
int f413076(TRUSERID *handle,ST_CPACK *rpack,ST_PACK *pArrays,int *iRetCode, char *szMsg)
{    
	ST_CPACK apack={0};
	ST_PACK arrays[100];        // 存放应答后续包组
	ST_PACK *pDataArray;
	T_UDATA UData;	
	char scust_no[31]={0};
	SetCol(handle,
		F_SNAME,
		F_SBANK_CODE2,
		F_DAMT0,
		F_SDATE0,
		F_LSAFE_LEVEL,
		END_FIELD);

	rpack->head.RequestType=130918;  //转换成快订的请求功能号

	strcpy(UData.sClientID,rpack->pack.sname);
	g_KSCTS.GetCTSCustNo(&UData,scust_no);
	AddFieldValue(rpack,"scust_no",scust_no);	//客户号
	
	
	if (g_pSvrLink->ExtCall(g_KSCTS.m_CTSSourceNo,g_KSCTS.m_CTSDestNo,g_KSCTS.m_CTSFuncNo,0,g_KSCTS.m_CTSAckTime,rpack,&apack,arrays)<0)
	{
		//则说明查询失败
		*iRetCode = 9988;
		strcpy(szMsg,"ErrNo=9988:Fail to Call KSCTS for Network!|调用KSCTS失败");
		return -1;
	}	
	while (1)
	{
		if (apack.head.retCode!=0)
		{
			*iRetCode = apack.head.retCode;
			sprintf(szMsg,"ErrNo=%d|%s",apack.head.retCode,apack.pack.vsmess);			
			return -1;
		}
		
		if(apack.head.recCount>=1)
		{
			*iRetCode=apack.head.retCode;
			for(int i=0;i<apack.head.recCount;++i)	
			{				
				if(i==0)
					pDataArray=&(apack.pack);
				else
					pDataArray=&(arrays[i-1]);
				strcpy(pArrays->sname,rpack->pack.scust_no);
				g_KSCTS.TransCurrency(pArrays->sbank_code2,pDataArray->scurrency_type,1);	//币种，传空，转换后也是空
				pArrays->damt0=pDataArray->damt0;
				strcpy(pArrays->sdate0,pDataArray->sdate0);
				pArrays->lsafe_level=1;		//默认给1
				PutRow(handle,pArrays,iRetCode,szMsg);				
			}
		}
		if (apack.head.nextflag==0)
			break;
		if (g_pSvrLink->ExtCallNext(g_KSCTS.m_CTSAckTime,&apack,arrays)<0)
		{
			//则说明查询失败
			*iRetCode = 9988;
			strcpy(szMsg,"ErrNo=9988:Fail to Call KSCTS for Network!|调用KSCTS失败");	
			return -1;
		}		
	}

    return 0;
}
