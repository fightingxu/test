/********************************************************************
	created:	2014/12/08
	filename: 	tpug\tpcts\f413076.cpp
	file path:	tpug\tpcts
	file base:	f413076
	file ext:	cpp
	author:		ZSJ
	
	purpose:	��ѯ�충��ʷ�ʽ�
	in:
	sname	Ͷ�������˺Ŷ����˺�	CHAR	20	char[21]	ClientID
	sdate0	����	CHAR	8	char[9]	yyyymmdd�ش�

	
	out:
	sname	Ͷ�������˺Ŷ����˺�	CHAR	20	char[21]
	sbank_code2	����	CHAR	6	char[7]
	sdate0	����	CHAR	8	char[9]
	damt0	��������	DECIMAL	18,4	double

		
	retCode	���ش���	INTEGER	4	unsigned int 
	vsmess	������Ϣ	VARCHAR	255	char[256]
					  
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

extern CSvrLink *g_pSvrLink;  // ��ҵ��������ĵ�����
int f413076(TRUSERID *handle,ST_CPACK *rpack,ST_PACK *pArrays,int *iRetCode, char *szMsg)
{    
	ST_CPACK apack={0};
	ST_PACK arrays[100];        // ���Ӧ���������
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

	rpack->head.RequestType=130918;  //ת���ɿ충�������ܺ�

	strcpy(UData.sClientID,rpack->pack.sname);
	g_KSCTS.GetCTSCustNo(&UData,scust_no);
	AddFieldValue(rpack,"scust_no",scust_no);	//�ͻ���
	
	
	if (g_pSvrLink->ExtCall(g_KSCTS.m_CTSSourceNo,g_KSCTS.m_CTSDestNo,g_KSCTS.m_CTSFuncNo,0,g_KSCTS.m_CTSAckTime,rpack,&apack,arrays)<0)
	{
		//��˵����ѯʧ��
		*iRetCode = 9988;
		strcpy(szMsg,"ErrNo=9988:Fail to Call KSCTS for Network!|����KSCTSʧ��");
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
				g_KSCTS.TransCurrency(pArrays->sbank_code2,pDataArray->scurrency_type,1);	//���֣����գ�ת����Ҳ�ǿ�
				pArrays->damt0=pDataArray->damt0;
				strcpy(pArrays->sdate0,pDataArray->sdate0);
				pArrays->lsafe_level=1;		//Ĭ�ϸ�1
				PutRow(handle,pArrays,iRetCode,szMsg);				
			}
		}
		if (apack.head.nextflag==0)
			break;
		if (g_pSvrLink->ExtCallNext(g_KSCTS.m_CTSAckTime,&apack,arrays)<0)
		{
			//��˵����ѯʧ��
			*iRetCode = 9988;
			strcpy(szMsg,"ErrNo=9988:Fail to Call KSCTS for Network!|����KSCTSʧ��");	
			return -1;
		}		
	}

    return 0;
}
