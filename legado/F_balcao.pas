unit F_balcao;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, ExtCtrls, Buttons, ComCtrls, Grids, DBGrids, pessoal,
  jpeg, Menus, RLPrinters, Data.DB, ppDesignLayer, ppParameter, ppCtrls,
  ppBands, ppVar, ppPrnabl, ppClass, ppCache, ppProd, ppReport, ppDB, ppComm,
  ppRelatv, ppDBPipe, Vcl.Mask, ToolWin, Winsock, IBX.IBCustomDataSet,
  IBX.IBQuery, IBX.IBDatabase, ACBrBase, ACBrNFeDANFEClass, ACBrNFeDANFeESCPOS,
  ACBrDFeReport, ACBrDFeDANFeReport, ACBrNFeDANFeRLClass, ACBrDFe, ACBrNFe,
  ACBrDANFCeFortesFrA4, ACBrSAT, ACBrSATClass, ACBrIntegrador,
  ACBrSATExtratoReportClass, ACBrSATExtratoFortesFr, pcnConversao,
  ACBrSATExtratoClass, ACBrSATExtratoESCPOS, ACBrUtil, ACBrPosPrinter,
  System.TypInfo, ACBrValidador, pcnConversaoNFe, System.Actions, Vcl.ActnList,
  F_Recebimentos, pcnRede, msgLib, Vcl.Imaging.pngimage, Datasnap.DBClient,
  RzEdit, ACBrBAL, F_PrecoAtacado, RLTypes, WinSpool, Vcl.Printers,blcksock,
  pcnNFe, ACBrNFeNotasFiscais, System.math, acbrDFEUtil, ACBrDFeSSL, System.StrUtils,
  FireDAC.Stan.Intf, FireDAC.Stan.Option, FireDAC.Stan.Param,
  FireDAC.Stan.Error, FireDAC.DatS, FireDAC.Phys.Intf, FireDAC.DApt.Intf,
  FireDAC.Stan.Async, FireDAC.DApt, FireDAC.Comp.DataSet, FireDAC.Comp.Client;

const
  InputBoxMessage = WM_USER + 200;

type
  TFbalcao = class(TForm)
    pnlDesktop: TPanel;
    pnlRight: TPanel;
    DBGrid1: TDBGrid;
    PopupMenu1: TPopupMenu;
    ExcluirItem1: TMenuItem;
    Timer1: TTimer;
    Frente_tmpitvendas: TppDBPipeline;
    Panel5: TPanel;
    Edit4: TEdit;
    Label17: TLabel;
    Label18: TLabel;
    Edit5: TEdit;
    Label19: TLabel;
    Edit6: TEdit;
    BitBtn5: TBitBtn;
    Timer2: TTimer;
    AlteraValorUnitrio1: TMenuItem;
    Panel9: TPanel;
    Label21: TLabel;
    Label22: TLabel;
    Label25: TLabel;
    Label26: TLabel;
    Label27: TLabel;
    Label28: TLabel;
    Label29: TLabel;
    Button2: TButton;
    AlterarValoraMenor1: TMenuItem;
    Label20: TLabel;
    Label32: TLabel;
    Edit7: TEdit;
    Edit8: TEdit;
    Edit9: TEdit;
    Button1: TButton;
    Panel10: TPanel;
    Label33: TLabel;
    Label34: TLabel;
    Label35: TLabel;
    Label36: TLabel;
    Label37: TLabel;
    Label38: TLabel;
    Label39: TLabel;
    Button3: TButton;
    Edit10: TEdit;
    Edit11: TEdit;
    Edit12: TEdit;
    Button4: TButton;
    Label40: TLabel;
    Edit13: TEdit;
    Panel11: TPanel;
    Edit14: TEdit;
    Edit15: TEdit;
    Label41: TLabel;
    Label42: TLabel;
    Label43: TLabel;
    BitBtn8: TBitBtn;
    Panel12: TPanel;
    Label45: TLabel;
    Edit16: TEdit;
    Label46: TLabel;
    BitBtn9: TBitBtn;
    Panel13: TPanel;
    Edit17: TEdit;
    Label48: TLabel;
    BitBtn10: TBitBtn;
    Label49: TLabel;
    Panel14: TPanel;
    RadioButton1: TRadioButton;
    RadioButton2: TRadioButton;
    Panel15: TPanel;
    BitBtn12: TBitBtn;
    ppReport2: TppReport;
    ppHeaderBand2: TppHeaderBand;
    ppDetailBand2: TppDetailBand;
    ppFooterBand2: TppFooterBand;
    ppSummaryBand1: TppSummaryBand;
    ppDBText7: TppDBText;
    ppDBText8: TppDBText;
    ppDBText9: TppDBText;
    ppDBText10: TppDBText;
    ppDBText12: TppDBText;
    ppLabel19: TppLabel;
    ppLabel20: TppLabel;
    ppLabel23: TppLabel;
    ppLabel24: TppLabel;
    ppLabel25: TppLabel;
    ppLabel26: TppLabel;
    ppLabel27: TppLabel;
    ppLabel29: TppLabel;
    ppSystemVariable2: TppSystemVariable;
    ppLine6: TppLine;
    ppLine7: TppLine;
    ppLine8: TppLine;
    ppLabel35: TppLabel;
    ppLabel30: TppLabel;
    ppLabel31: TppLabel;
    ppLabel32: TppLabel;
    ppLabel33: TppLabel;
    ppLabel34: TppLabel;
    ppLine9: TppLine;
    ppLabel36: TppLabel;
    ppDBCalc2: TppDBCalc;
    ppLabel37: TppLabel;
    ppLabel38: TppLabel;
    ppLabel39: TppLabel;
    ppLabel40: TppLabel;
    ppLabel41: TppLabel;
    Image1: TImage;
    Panel16: TPanel;
    StatusBar2: TStatusBar;
    Edit18: TEdit;
    Label54: TLabel;
    Label53: TLabel;
    Button6: TButton;
    Panel17: TPanel;
    StatusBar3: TStatusBar;
    Panel18: TPanel;
    StatusBar4: TStatusBar;
    MaskEdit1: TMaskEdit;
    Panel20: TPanel;
    Shape1: TShape;
    StatusBar5: TStatusBar;
    Shape2: TShape;
    Label55: TLabel;
    BitBtn13: TBitBtn;
    Label56: TLabel;
    Edit19: TEdit;
    Vale: TppReport;
    ppHeaderBand4: TppHeaderBand;
    ppShape19: TppShape;
    ppImage2: TppImage;
    ppLabel90: TppLabel;
    ppLabel91: TppLabel;
    ppLabel92: TppLabel;
    ppLabel93: TppLabel;
    ppDetailBand4: TppDetailBand;
    ppFooterBand4: TppFooterBand;
    Label57: TLabel;
    Edit20: TEdit;
    Label59: TLabel;
    ppLabel1: TppLabel;
    ppLabel2: TppLabel;
    ppLabel3: TppLabel;
    Panel21: TPanel;
    StatusBar6: TStatusBar;
    Panel22: TPanel;
    Edit21: TEdit;
    Label60: TLabel;
    BitBtn15: TBitBtn;
    BitBtn16: TBitBtn;
    qryConsulta: TIBQuery;
    qryExecutar: TIBQuery;
    qryTMPItens: TIBQuery;
    qryProduto: TIBQuery;
    DSTmpItens: TDataSource;
    qryVendedor: TIBQuery;
    qryVendas: TIBQuery;
    qryVendasNOTA: TIBStringField;
    qryVendasMODELO: TIBStringField;
    qryVendasSERIE: TIBStringField;
    qryVendasSUBSERIE: TIBStringField;
    qryVendasORIGEM: TIBStringField;
    qryVendasEMISSAO: TDateField;
    qryVendasHORA: TTimeField;
    qryVendasENTRADA: TIBStringField;
    qryVendasSAIDA: TIBStringField;
    qryVendasCFOPS: TIBStringField;
    qryVendasNATUREZA: TIBStringField;
    qryVendasCLIENTE: TIntegerField;
    qryVendasIMPORTACOES: TWideMemoField;
    qryVendasDATA_SAIDA: TDateField;
    qryVendasHORA_SAIDA: TTimeField;
    qryVendasOBS: TWideMemoField;
    qryVendasADICIONAIS: TWideMemoField;
    qryVendasTRANSP_1: TIntegerField;
    qryVendasTRANSP_2: TIntegerField;
    qryVendasLOJA: TIBStringField;
    qryVendasVENDEDOR: TIntegerField;
    qryVendasFORMAS_PGTO: TIBStringField;
    qryVendasQUANTIDADE: TIBStringField;
    qryVendasESPECIE: TIBStringField;
    qryVendasMARCA: TIBStringField;
    qryVendasPBRUTO: TIBStringField;
    qryVendasPLIQUIDO: TIBStringField;
    qryVendasFRETE_1: TIBStringField;
    qryVendasFRETE_2: TIBStringField;
    qryVendasANEXA_ICMS_SUB: TSmallintField;
    qryVendasCOMISSAO: TIBBCDField;
    qryVendasDESCONTO: TIBBCDField;
    qryVendasACRESCIMO: TIBBCDField;
    qryVendasICMS_FRETE: TIBBCDField;
    qryVendasBASE_ICMS: TIBBCDField;
    qryVendasBASE_ICMS_SUB: TIBBCDField;
    qryVendasBASE_IPI: TIBBCDField;
    qryVendasTOT_ICMS: TIBBCDField;
    qryVendasTOT_ICMS_SUB: TIBBCDField;
    qryVendasTOT_IPI: TIBBCDField;
    qryVendasTOT_SERVICOS: TIBBCDField;
    qryVendasTOT_ISS: TIBBCDField;
    qryVendasTOT_PRODUTOS: TIBBCDField;
    qryVendasTOT_IRRF: TIBBCDField;
    qryVendasTOT_PIS: TIBBCDField;
    qryVendasTOT_COFINS: TIBBCDField;
    qryVendasTOT_CS: TIBBCDField;
    qryVendasTOT_SER_BRUTO: TIBBCDField;
    qryVendasTOT_SEGURO: TIBBCDField;
    qryVendasTOT_OUTRAS: TIBBCDField;
    qryVendasTOT_FRETE: TIBBCDField;
    qryVendasCANCELADO: TSmallintField;
    qryVendasTOTAL: TIBBCDField;
    qryVendasOPERADOR: TIntegerField;
    qryVendasSEQUENCIA: TIntegerField;
    qryVendasCONVENIO: TIBBCDField;
    qryVendasNSU: TIntegerField;
    qryVendasDATA_IMPRESSAO: TDateField;
    qryVendasHORA_IMPRESSAO: TTimeField;
    qryVendasAVISTA: TSmallintField;
    qryVendasTOT_ICMS_FRETE: TIBBCDField;
    qryVendasTOT_CONVENIO: TIBBCDField;
    qryVendasTAB_PRECO: TIntegerField;
    qryVendasANEXA_ST: TSmallintField;
    qryVendasTOTALIZA_CFOP: TIBStringField;
    qryVendasCONTA: TIntegerField;
    qryVendasCUSTO: TIntegerField;
    qryVendasSERIAIS_PROD: TWideMemoField;
    qryVendasNFE_FLDE: TIBStringField;
    qryVendasNFE_STATUS: TIBStringField;
    qryVendasTOT_IMPORTACAO: TIBBCDField;
    qryVendasNF_REFERENCIADA: TIntegerField;
    qryVendasTOT_PIS_SUB: TIBBCDField;
    qryVendasTOT_COFINS_SUB: TIBBCDField;
    qryVendasANEXA_ICMS_FRETE: TSmallintField;
    qryVendasPLACA_TRANSP1: TIBStringField;
    qryVendasIMPORTADOS: TIBStringField;
    qryVendasPLACA_TRANSP2: TIBStringField;
    qryVendasANEXA_SEGURO: TSmallintField;
    qryVendasANEXA_OUTRAS: TSmallintField;
    qryVendasANTT_TRANSP1: TIBStringField;
    qryVendasANTT_TRANSP2: TIBStringField;
    qryVendasUFEMBARQ: TIBStringField;
    qryVendasXLOCEMBARQ: TIBStringField;
    qryVendasTOT_ICMS_OUTRAS_DESP: TIBBCDField;
    qryVendasTOT_ICMS_SEGURO: TIBBCDField;
    qryVendasANEXA_FRETE: TSmallintField;
    qryVendasIE_SUBSTITUTO: TIBStringField;
    qryVendasPIS_VALOR_BC: TIBBCDField;
    qryVendasPIS_VALOR_BC_SUB: TIBBCDField;
    qryVendasCOFINS_VALOR_BC: TIBBCDField;
    qryVendasCOFINS_VALOR_BC_SUB: TIBBCDField;
    qryVendasMOTIVO_D2_POSTO: TSmallintField;
    qryVendasAAMMNFP: TDateField;
    qryVendasNNFNFP: TIBStringField;
    qryVendasMODNFP: TIBStringField;
    qryVendasSERIENFP: TIBStringField;
    qryVendasNUMERO: TIBStringField;
    qryVendasVTOT_TRIB: TIBBCDField;
    qryVendasESPECIE_PAGAMENTO: TIBStringField;
    qryVendasPAF: TSmallintField;
    qryVendasIND_PRES: TSmallintField;
    qryVendasREFNFE: TIBStringField;
    qryVendasPLACA: TIBStringField;
    qryVendasMODELOCAR: TIBStringField;
    qryVendasSTATUS: TIBStringField;
    qryVendasDINHEIRO: TIBBCDField;
    qryVendasCHEQUE: TIBBCDField;
    qryVendasCARTAO: TIBBCDField;
    qryVendasBOLETO: TIBBCDField;
    qryVendasTROCO: TIBBCDField;
    qryVendasCODPROF: TIntegerField;
    qryVendasOS: TIBStringField;
    qryVendasVALE: TIBBCDField;
    qryVendasLANCADO: TIBStringField;
    qryVendasCARTAOD: TIBBCDField;
    qryVendasKM: TIBStringField;
    qryVendasDESTINO: TIBStringField;
    qryVendasCPF_NOTA: TIBStringField;
    qryRecVendas: TIBQuery;
    qryRecVendasID: TIntegerField;
    qryRecVendasID_FORMA_PAGAMENTO: TIntegerField;
    qryRecVendasN_CAIXA: TIntegerField;
    qryRecVendasNOTA: TIBStringField;
    qryRecVendasVALOR: TFloatField;
    qryRecVendasTROCO: TIBBCDField;
    qryRecVendasTIPO: TIBStringField;
    qryItem: TIBQuery;
    qryItemCUPOM: TIBStringField;
    qryItemN_CAIXA: TIntegerField;
    qryItemDATA: TDateField;
    qryItemHORA: TTimeField;
    qryItemOPERADOR: TIntegerField;
    qryItemITEM: TIntegerField;
    qryItemCODIGO: TIntegerField;
    qryItemBARRAS: TIBStringField;
    qryItemDESCRICAO: TIBStringField;
    qryItemQTD: TIBBCDField;
    qryItemPRECO: TIBBCDField;
    qryItemTRIBUTACAO: TIBStringField;
    qryItemICMS: TIBBCDField;
    qryItemISS: TIBBCDField;
    qryItemUND: TIBStringField;
    qryItemGRADE_X: TWideMemoField;
    qryItemGRADE_Y: TWideMemoField;
    qryItemGRADE_QUA: TWideMemoField;
    qryItemGRADE_VENDIDA: TWideMemoField;
    qryItemSERIAL: TIBStringField;
    qryItemDESCONTO: TIBBCDField;
    qryItemACRESCIMO: TIBBCDField;
    qryItemTOTAL: TIBBCDField;
    qryItemCANCELADO: TSmallintField;
    qryItemOPERADOR_SUP: TIntegerField;
    qryItemLOTE: TIBStringField;
    qryItemTIPO: TSmallintField;
    qryItemTABELA_PRECO: TIBStringField;
    qryItemGRUPO: TIntegerField;
    qryItemCUSTO: TIBBCDField;
    qryItemCODCAR: TIntegerField;
    qryItemMODELO: TIBStringField;
    qryItemCODESTCAR: TIntegerField;
    qryItemMODCAR: TIBStringField;
    qryItemNCM: TIBStringField;
    qryItemCST: TIBStringField;
    qryItemCFOP: TIBStringField;
    qryItemCODPRO_VEIC: TIntegerField;
    qryItemCSOSN: TIBStringField;
    qryItemCST_ORIGEM: TIBStringField;
    qryItemVALOR_ICMS: TIBBCDField;
    qryItemCODGRADE: TIntegerField;
    qryItemTIPO_TELA: TIBStringField;
    qryProd: TIBQuery;
    qryProdID: TIntegerField;
    qryProdCODIGOBARRA: TIBStringField;
    qryProdQUANTIDADE: TIBBCDField;
    qryProdDESCRICAO: TIBStringField;
    qryProdUN_MEDIDA: TIBStringField;
    qryProdPRECOCUSTO: TIBBCDField;
    qryProdPRECOVENDA: TIBBCDField;
    qryProdATACADO: TIBBCDField;
    qryProdCST_ORIGEM: TIBStringField;
    qryProdCST: TIBStringField;
    qryProdCSOSN: TIBStringField;
    qryProdICMS: TIBBCDField;
    qryProdICMS_ST: TIBBCDField;
    qryProdNCM: TIBStringField;
    qryProdCEST: TIBStringField;
    qryProdCFOP: TIBStringField;
    qryIBPT: TIBQuery;
    qrySomaNFCe: TIBQuery;
    qrySomaNFCeTOTAL: TIBBCDField;
    qrySomaNFCeBASE_ICMS: TIBBCDField;
    qrySomaNFCeVALOR_ICMS: TIBBCDField;
    qrySomaNFCeBASE_PIS_ICMS: TIBBCDField;
    qrySomaNFCeVALOR_PIS_ICMS: TIBBCDField;
    qrySomaNFCeBASE_COF_ICMS: TIBBCDField;
    qrySomaNFCeVALOR_COF_ICMS: TIBBCDField;
    qrySomaNFCeBASE_ISS: TIBBCDField;
    qrySomaNFCeVALOR_ISS: TIBBCDField;
    qrySomaNFCeTOTALMUN: TIBBCDField;
    qrySomaNFCeTOTALFED: TIBBCDField;
    qrySomaNFCeTOTALEST: TIBBCDField;
    qrySomaNFCeTOTALIMP: TIBBCDField;
    qrySomaNFCeDESCONTOS: TIBBCDField;
    qrySomaNFCeOUTROS: TIBBCDField;
    qryIBPTCODIGO: TIBStringField;
    qryIBPTEX: TIBStringField;
    qryIBPTTIPO: TIBStringField;
    qryIBPTDESCRICAO: TIBStringField;
    qryIBPTNACIONALFEDERAL: TFloatField;
    qryIBPTIMPORTADOSFEDERAL: TFloatField;
    qryIBPTESTADUAL: TFloatField;
    qryIBPTMUNICIPAL: TFloatField;
    qryIBPTVIGENCIAINICIO: TDateField;
    qryIBPTVIGENCIAFIM: TDateField;
    qryIBPTCHAVE: TIBStringField;
    qryIBPTVERSAO: TIBStringField;
    qryIBPTFONTE: TIBStringField;
    qryProdCST_PIS: TIBStringField;
    qryProdALIQ_PIS: TIBBCDField;
    qryProdCST_COFINS: TIBStringField;
    qryProdALIQ_COFINS: TIBBCDField;
    ActionList1: TActionList;
    actModoEmissao: TAction;
    pnlFormaPgto: TPanel;
    Panel23: TPanel;
    Panel41: TPanel;
    listMetodoPagto: TListBox;
    qryTMPReceb: TIBQuery;
    ACBrValidador1: TACBrValidador;
    pnlBotoes: TPanel;
    BitBtn1: TBitBtn;
    BitBtn2: TBitBtn;
    BitBtn3: TBitBtn;
    BitBtn4: TBitBtn;
    BitBtn6: TBitBtn;
    BitBtn7: TBitBtn;
    BitBtn11: TBitBtn;
    imgDoc: TImage;
    imgImp: TImage;
    pnlTop: TPanel;
    pnlCaixaFechado: TPanel;
    Label4: TLabel;
    lblData: TLabel;
    lblHora: TLabel;
    Image2: TImage;
    Label3: TLabel;
    pnlClient: TPanel;
    pnlTotais: TPanel;
    Label58: TLabel;
    Label50: TLabel;
    Label51: TLabel;
    LABEL1: TStaticText;
    StaticText2: TStaticText;
    Panel3: TPanel;
    Label2: TLabel;
    Label44: TLabel;
    Label47: TLabel;
    Label52: TLabel;
    StaticText1: TStaticText;
    Edit1: TEdit;
    Edit2: TEdit;
    StaticText4: TStaticText;
    Edit3: TEdit;
    StaticText5: TStaticText;
    StaticText3: TStaticText;
    ComboBox1: TComboBox;
    stHora: TStaticText;
    stData: TStaticText;
    Panel6: TPanel;
    Label13: TLabel;
    Label14: TLabel;
    Label15: TLabel;
    Label16: TLabel;
    stMaquinaIP: TStaticText;
    stVersao: TStaticText;
    Label9: TLabel;
    lblConsumidor: TLabel;
    Label11: TLabel;
    Label12: TLabel;
    qryLimite: TIBQuery;
    pnlValor: TPanel;
    edtValor: TRzNumericEdit;
    qryClientes: TIBQuery;
    ACBrBAL1: TACBrBAL;
    Boleto: TppReport;
    ppHeaderBand3: TppHeaderBand;
    ppDetailBand3: TppDetailBand;
    ppShape17: TppShape;
    ppShape18: TppShape;
    ppShape1: TppShape;
    ppShape20: TppShape;
    ppLabel59: TppLabel;
    ppLabel60: TppLabel;
    ppLabel61: TppLabel;
    ppShape21: TppShape;
    ppLabel62: TppLabel;
    ppLabel63: TppLabel;
    ppLabel64: TppLabel;
    ppLabel65: TppLabel;
    ppLabel66: TppLabel;
    ppLabel67: TppLabel;
    ppShape22: TppShape;
    ppShape23: TppShape;
    ppShape24: TppShape;
    ppLabel68: TppLabel;
    ppLabel69: TppLabel;
    ppLabel70: TppLabel;
    ppLabel71: TppLabel;
    ppLabel72: TppLabel;
    ppLabel73: TppLabel;
    ppLabel74: TppLabel;
    ppLabel75: TppLabel;
    ppLabel77: TppLabel;
    ppLabel78: TppLabel;
    ppLabel79: TppLabel;
    ppLabel80: TppLabel;
    ppLabel83: TppLabel;
    ppLabel84: TppLabel;
    ppLabel88: TppLabel;
    ppLabel89: TppLabel;
    ppLabel4: TppLabel;
    ppLabel5: TppLabel;
    ppLabel6: TppLabel;
    ppLabel7: TppLabel;
    ppLabel94: TppLabel;
    ppLabel95: TppLabel;
    ppLabel96: TppLabel;
    ppLine1: TppLine;
    ppLabel97: TppLabel;
    ppLabel98: TppLabel;
    ppLabel99: TppLabel;
    ppLabel100: TppLabel;
    ppDBText1: TppDBText;
    ppDBText2: TppDBText;
    ppDBText16: TppDBText;
    ppLabel105: TppLabel;
    ppDBText17: TppDBText;
    ppDBText18: TppDBText;
    ppDBText11: TppDBText;
    ppLabel81: TppLabel;
    ppDBText19: TppDBText;
    ppFooterBand3: TppFooterBand;
    ppDesignLayers3: TppDesignLayers;
    ppDesignLayer4: TppDesignLayer;
    ppParameterList3: TppParameterList;
    Promissoria: TppReport;
    ppHeaderBand6: TppHeaderBand;
    ppDetailBand8: TppDetailBand;
    ppShape38: TppShape;
    ppShape42: TppShape;
    ppShape39: TppShape;
    ppLabel164: TppLabel;
    ppLine23: TppLine;
    ppLine24: TppLine;
    ppLine25: TppLine;
    ppLabel165: TppLabel;
    ppLabel166: TppLabel;
    ppLabel167: TppLabel;
    ppLabel168: TppLabel;
    ppLabel170: TppLabel;
    ppLabel171: TppLabel;
    ppLabel173: TppLabel;
    ppLabel175: TppLabel;
    ppLine26: TppLine;
    ppLabel176: TppLabel;
    ppShape40: TppShape;
    ppLabel177: TppLabel;
    ppLabel178: TppLabel;
    ppShape41: TppShape;
    ppLabel179: TppLabel;
    ppLabel180: TppLabel;
    ppLabel181: TppLabel;
    ppLabel182: TppLabel;
    ppLabel183: TppLabel;
    ppLabel191: TppLabel;
    ppLine28: TppLine;
    ppLabel184: TppLabel;
    ppLabel187: TppLabel;
    ppLabel188: TppLabel;
    ppLabel101: TppLabel;
    ppLabel102: TppLabel;
    ppLabel103: TppLabel;
    ppLabel104: TppLabel;
    ppImage1: TppImage;
    ppDBText20: TppDBText;
    ppDBText21: TppDBText;
    ppFooterBand6: TppFooterBand;
    ppDesignLayers4: TppDesignLayers;
    ppDesignLayer5: TppDesignLayer;
    ppParameterList4: TppParameterList;
    qryreceber: TIBQuery;
    ppDBPipeline1: TppDBPipeline;
    DSQryReceber: TDataSource;
    ppDBText3: TppDBText;
    qryFiscal: TIBQuery;
    Label5: TLabel;
    Label6: TLabel;
    Label7: TLabel;
    Label8: TLabel;
    Label23: TLabel;
    qryItemCOMPETENCIA: TIBStringField;
    qryItemPRECOCUSTO: TIBBCDField;
    Label24: TLabel;
    qryConsulta01: TIBQuery;
    DSqryConsulta01: TDataSource;
    DSqryConsulta: TDataSource;
    qryItensTeste: TIBQuery;
    qryProdItem: TIBQuery;
    Label30: TLabel;
    ACBrSAT1: TACBrSAT;
    ACBrSATExtratoESCPOS1: TACBrSATExtratoESCPOS;
    ACBrSATExtratoFortes1: TACBrSATExtratoFortes;
    ACBrPosPrinter1: TACBrPosPrinter;
    lblstatus: TLabel;
    qryRecebimento: TIBQuery;
    Label31: TLabel;
    Label61: TLabel;
    Label62: TLabel;
    qryItemMARCA: TIBStringField;
    qryItemQTDBAIXA: TIBBCDField;
    qryItemDESCONTO1: TIBBCDField;
    lb_CpfCnpj: TLabel;
    Label63: TLabel;
    qryAltera: TIBQuery;
    qryBuscaQTD: TIBQuery;
    StaticText6: TStaticText;
    edtComanda: TEdit;
    StaticText7: TStaticText;
    edtMesa: TEdit;
    Vendas: TIBDataSet;
    actBuscarComanda: TAction;
    actSalvarComanda: TAction;
    VendasNOTA: TIBStringField;
    VendasMODELO: TIBStringField;
    VendasSERIE: TIBStringField;
    VendasSUBSERIE: TIBStringField;
    VendasORIGEM: TIBStringField;
    VendasEMISSAO: TDateField;
    VendasHORA: TTimeField;
    VendasENTRADA: TIBStringField;
    VendasSAIDA: TIBStringField;
    VendasCFOPS: TIBStringField;
    VendasNATUREZA: TIBStringField;
    VendasCLIENTE: TIntegerField;
    VendasIMPORTACOES: TWideMemoField;
    VendasDATA_SAIDA: TDateField;
    VendasHORA_SAIDA: TTimeField;
    VendasOBS: TWideMemoField;
    VendasADICIONAIS: TWideMemoField;
    VendasTRANSP_1: TIntegerField;
    VendasTRANSP_2: TIntegerField;
    VendasLOJA: TIBStringField;
    VendasVENDEDOR: TIntegerField;
    VendasFORMAS_PGTO: TIBStringField;
    VendasQUANTIDADE: TIBStringField;
    VendasESPECIE: TIBStringField;
    VendasMARCA: TIBStringField;
    VendasPBRUTO: TIBStringField;
    VendasPLIQUIDO: TIBStringField;
    VendasFRETE_1: TIBStringField;
    VendasFRETE_2: TIBStringField;
    VendasANEXA_ICMS_SUB: TSmallintField;
    VendasCOMISSAO: TIBBCDField;
    VendasDESCONTO: TIBBCDField;
    VendasACRESCIMO: TIBBCDField;
    VendasICMS_FRETE: TIBBCDField;
    VendasBASE_ICMS: TIBBCDField;
    VendasBASE_ICMS_SUB: TIBBCDField;
    VendasBASE_IPI: TIBBCDField;
    VendasTOT_ICMS: TIBBCDField;
    VendasTOT_ICMS_SUB: TIBBCDField;
    VendasTOT_IPI: TIBBCDField;
    VendasTOT_SERVICOS: TIBBCDField;
    VendasTOT_ISS: TIBBCDField;
    VendasTOT_PRODUTOS: TIBBCDField;
    VendasTOT_IRRF: TIBBCDField;
    VendasTOT_PIS: TIBBCDField;
    VendasTOT_COFINS: TIBBCDField;
    VendasTOT_CS: TIBBCDField;
    VendasTOT_SER_BRUTO: TIBBCDField;
    VendasTOT_SEGURO: TIBBCDField;
    VendasTOT_OUTRAS: TIBBCDField;
    VendasTOT_FRETE: TIBBCDField;
    VendasCANCELADO: TSmallintField;
    VendasTOTAL: TIBBCDField;
    VendasOPERADOR: TIntegerField;
    VendasSEQUENCIA: TIntegerField;
    VendasCONVENIO: TIBBCDField;
    VendasNSU: TIntegerField;
    VendasDATA_IMPRESSAO: TDateField;
    VendasHORA_IMPRESSAO: TTimeField;
    VendasAVISTA: TSmallintField;
    VendasTOT_ICMS_FRETE: TIBBCDField;
    VendasTOT_CONVENIO: TIBBCDField;
    VendasTAB_PRECO: TIntegerField;
    VendasANEXA_ST: TSmallintField;
    VendasTOTALIZA_CFOP: TIBStringField;
    VendasCONTA: TIntegerField;
    VendasCUSTO: TIntegerField;
    VendasSERIAIS_PROD: TWideMemoField;
    VendasNFE_FLDE: TIBStringField;
    VendasNFE_STATUS: TIBStringField;
    VendasTOT_IMPORTACAO: TIBBCDField;
    VendasNF_REFERENCIADA: TIntegerField;
    VendasTOT_PIS_SUB: TIBBCDField;
    VendasTOT_COFINS_SUB: TIBBCDField;
    VendasANEXA_ICMS_FRETE: TSmallintField;
    VendasPLACA_TRANSP1: TIBStringField;
    VendasIMPORTADOS: TIBStringField;
    VendasPLACA_TRANSP2: TIBStringField;
    VendasANEXA_SEGURO: TSmallintField;
    VendasANEXA_OUTRAS: TSmallintField;
    VendasANTT_TRANSP1: TIBStringField;
    VendasANTT_TRANSP2: TIBStringField;
    VendasUFEMBARQ: TIBStringField;
    VendasXLOCEMBARQ: TIBStringField;
    VendasTOT_ICMS_OUTRAS_DESP: TIBBCDField;
    VendasTOT_ICMS_SEGURO: TIBBCDField;
    VendasANEXA_FRETE: TSmallintField;
    VendasIE_SUBSTITUTO: TIBStringField;
    VendasPIS_VALOR_BC: TIBBCDField;
    VendasPIS_VALOR_BC_SUB: TIBBCDField;
    VendasCOFINS_VALOR_BC: TIBBCDField;
    VendasCOFINS_VALOR_BC_SUB: TIBBCDField;
    VendasMOTIVO_D2_POSTO: TSmallintField;
    VendasAAMMNFP: TDateField;
    VendasNNFNFP: TIBStringField;
    VendasMODNFP: TIBStringField;
    VendasSERIENFP: TIBStringField;
    VendasNUMERO: TIBStringField;
    VendasVTOT_TRIB: TIBBCDField;
    VendasESPECIE_PAGAMENTO: TIBStringField;
    VendasPAF: TSmallintField;
    VendasIND_PRES: TSmallintField;
    VendasREFNFE: TIBStringField;
    VendasPLACA: TIBStringField;
    VendasMODELOCAR: TIBStringField;
    VendasSTATUS: TIBStringField;
    VendasDINHEIRO: TIBBCDField;
    VendasCHEQUE: TIBBCDField;
    VendasCARTAO: TIBBCDField;
    VendasBOLETO: TIBBCDField;
    VendasTROCO: TIBBCDField;
    VendasCODPROF: TIntegerField;
    VendasOS: TIBStringField;
    VendasVALE: TIBBCDField;
    VendasLANCADO: TIBStringField;
    VendasCARTAOD: TIBBCDField;
    VendasKM: TIBStringField;
    VendasDESTINO: TIBStringField;
    VendasWHAT: TIBStringField;
    VendasCPF_NOTA: TIBStringField;
    VendasCAIXA: TIntegerField;
    VendasTIPO_TRANSACAO: TIntegerField;
    VendasDATA_CANCELA: TDateField;
    VendasPIX: TIBBCDField;
    VendasHORA_CANCELA: TTimeField;
    VendasUSUARIO_CANCELA: TIntegerField;
    VendasJUST_CANCELA: TIBStringField;
    VendasFUNCIONARIO01: TIBStringField;
    VendasFUNCIONARIO02: TIBStringField;
    VendasNOMEPDV: TIBStringField;
    VendasID_OBRAS: TIntegerField;
    VendasN_MESA: TIntegerField;
    VendasN_COMANDA: TIntegerField;
    ppCupom: TppReport;
    ppHeaderBand1: TppHeaderBand;
    ppFantasia: TppLabel;
    ppEndereco: TppLabel;
    ppCnpj: TppLabel;
    ppLine2: TppLine;
    ppData: TppLabel;
    ppEspecie: TppLabel;
    ppLabel82: TppLabel;
    ppLabel85: TppLabel;
    ppLine3: TppLine;
    ppDetailBand1: TppDetailBand;
    ppDBText22: TppDBText;
    ppDBText23: TppDBText;
    ppDBText24: TppDBText;
    ppDBText25: TppDBText;
    ppDBText26: TppDBText;
    ppDBText27: TppDBText;
    ppDBText28: TppDBText;
    ppDBText29: TppDBText;
    ppFooterBand1: TppFooterBand;
    ppShape25: TppShape;
    ppLabel86: TppLabel;
    ppLabel87: TppLabel;
    ppLabel106: TppLabel;
    ppLabel107: TppLabel;
    ppqtd: TppLabel;
    pptotal: TppLabel;
    pptotalpago: TppLabel;
    pptroco: TppLabel;
    ppLabel112: TppLabel;
    ppLine4: TppLine;
    ppLine5: TppLine;
    ppLabel113: TppLabel;
    ppDesignLayers5: TppDesignLayers;
    ppDesignLayer6: TppDesignLayer;
    ppParameterList5: TppParameterList;
    ppDBCupom: TppDBPipeline;
    Vendas2: TFDQuery;
    Vendas2NOTA: TStringField;
    Vendas2MODELO: TStringField;
    Vendas2SERIE: TStringField;
    Vendas2SUBSERIE: TStringField;
    Vendas2ORIGEM: TStringField;
    Vendas2EMISSAO: TDateField;
    Vendas2HORA: TTimeField;
    Vendas2ENTRADA: TStringField;
    Vendas2SAIDA: TStringField;
    Vendas2CFOPS: TStringField;
    Vendas2NATUREZA: TStringField;
    Vendas2CLIENTE: TIntegerField;
    Vendas2IMPORTACOES: TMemoField;
    Vendas2DATA_SAIDA: TDateField;
    Vendas2HORA_SAIDA: TTimeField;
    Vendas2OBS: TMemoField;
    Vendas2ADICIONAIS: TMemoField;
    Vendas2TRANSP_1: TIntegerField;
    Vendas2TRANSP_2: TIntegerField;
    Vendas2LOJA: TStringField;
    Vendas2VENDEDOR: TIntegerField;
    Vendas2FORMAS_PGTO: TStringField;
    Vendas2QUANTIDADE: TStringField;
    Vendas2ESPECIE: TStringField;
    Vendas2MARCA: TStringField;
    Vendas2PBRUTO: TStringField;
    Vendas2PLIQUIDO: TStringField;
    Vendas2FRETE_1: TStringField;
    Vendas2FRETE_2: TStringField;
    Vendas2ANEXA_ICMS_SUB: TSmallintField;
    Vendas2COMISSAO: TBCDField;
    Vendas2DESCONTO: TBCDField;
    Vendas2ACRESCIMO: TBCDField;
    Vendas2ICMS_FRETE: TBCDField;
    Vendas2BASE_ICMS: TBCDField;
    Vendas2BASE_ICMS_SUB: TBCDField;
    Vendas2BASE_IPI: TBCDField;
    Vendas2TOT_ICMS: TBCDField;
    Vendas2TOT_ICMS_SUB: TBCDField;
    Vendas2TOT_IPI: TBCDField;
    Vendas2TOT_SERVICOS: TBCDField;
    Vendas2TOT_ISS: TBCDField;
    Vendas2TOT_PRODUTOS: TBCDField;
    Vendas2TOT_IRRF: TBCDField;
    Vendas2TOT_PIS: TBCDField;
    Vendas2TOT_COFINS: TBCDField;
    Vendas2TOT_CS: TBCDField;
    Vendas2TOT_SER_BRUTO: TBCDField;
    Vendas2TOT_SEGURO: TBCDField;
    Vendas2TOT_OUTRAS: TBCDField;
    Vendas2TOT_FRETE: TBCDField;
    Vendas2CANCELADO: TSmallintField;
    Vendas2TOTAL: TBCDField;
    Vendas2OPERADOR: TIntegerField;
    Vendas2SEQUENCIA: TIntegerField;
    Vendas2CONVENIO: TBCDField;
    Vendas2NSU: TIntegerField;
    Vendas2DATA_IMPRESSAO: TDateField;
    Vendas2HORA_IMPRESSAO: TTimeField;
    Vendas2AVISTA: TSmallintField;
    Vendas2TOT_ICMS_FRETE: TBCDField;
    Vendas2TOT_CONVENIO: TBCDField;
    Vendas2TAB_PRECO: TIntegerField;
    Vendas2ANEXA_ST: TSmallintField;
    Vendas2TOTALIZA_CFOP: TStringField;
    Vendas2CONTA: TIntegerField;
    Vendas2CUSTO: TIntegerField;
    Vendas2SERIAIS_PROD: TMemoField;
    Vendas2NFE_FLDE: TStringField;
    Vendas2NFE_STATUS: TStringField;
    Vendas2TOT_IMPORTACAO: TBCDField;
    Vendas2NF_REFERENCIADA: TIntegerField;
    Vendas2TOT_PIS_SUB: TBCDField;
    Vendas2TOT_COFINS_SUB: TBCDField;
    Vendas2ANEXA_ICMS_FRETE: TSmallintField;
    Vendas2PLACA_TRANSP1: TStringField;
    Vendas2IMPORTADOS: TStringField;
    Vendas2PLACA_TRANSP2: TStringField;
    Vendas2ANEXA_SEGURO: TSmallintField;
    Vendas2ANEXA_OUTRAS: TSmallintField;
    Vendas2ANTT_TRANSP1: TStringField;
    Vendas2ANTT_TRANSP2: TStringField;
    Vendas2UFEMBARQ: TStringField;
    Vendas2XLOCEMBARQ: TStringField;
    Vendas2TOT_ICMS_OUTRAS_DESP: TBCDField;
    Vendas2TOT_ICMS_SEGURO: TBCDField;
    Vendas2ANEXA_FRETE: TSmallintField;
    Vendas2IE_SUBSTITUTO: TStringField;
    Vendas2PIS_VALOR_BC: TBCDField;
    Vendas2PIS_VALOR_BC_SUB: TBCDField;
    Vendas2COFINS_VALOR_BC: TBCDField;
    Vendas2COFINS_VALOR_BC_SUB: TBCDField;
    Vendas2MOTIVO_D2_POSTO: TSmallintField;
    Vendas2AAMMNFP: TDateField;
    Vendas2NNFNFP: TStringField;
    Vendas2MODNFP: TStringField;
    Vendas2SERIENFP: TStringField;
    Vendas2NUMERO: TStringField;
    Vendas2VTOT_TRIB: TBCDField;
    Vendas2ESPECIE_PAGAMENTO: TStringField;
    Vendas2PAF: TSmallintField;
    Vendas2IND_PRES: TSmallintField;
    Vendas2REFNFE: TStringField;
    Vendas2PLACA: TWideStringField;
    Vendas2MODELOCAR: TWideStringField;
    Vendas2STATUS: TWideStringField;
    Vendas2DINHEIRO: TBCDField;
    Vendas2CHEQUE: TBCDField;
    Vendas2CARTAO: TBCDField;
    Vendas2BOLETO: TBCDField;
    Vendas2TROCO: TBCDField;
    Vendas2CODPROF: TIntegerField;
    Vendas2OS: TWideStringField;
    Vendas2VALE: TBCDField;
    Vendas2LANCADO: TWideStringField;
    Vendas2CARTAOD: TBCDField;
    Vendas2KM: TStringField;
    Vendas2DESTINO: TWideStringField;
    Vendas2WHAT: TStringField;
    Vendas2CPF_NOTA: TWideStringField;
    Vendas2CAIXA: TIntegerField;
    Vendas2TIPO_TRANSACAO: TIntegerField;
    Vendas2PIX: TBCDField;
    Vendas2DATA_CANCELA: TDateField;
    Vendas2HORA_CANCELA: TTimeField;
    Vendas2USUARIO_CANCELA: TIntegerField;
    Vendas2JUST_CANCELA: TWideStringField;
    Vendas2FUNCIONARIO01: TWideStringField;
    Vendas2FUNCIONARIO02: TWideStringField;
    Vendas2NOMEPDV: TWideStringField;
    Vendas2ID_OBRAS: TIntegerField;
    Vendas2N_MESA: TIntegerField;
    Vendas2N_COMANDA: TIntegerField;
    qryPagarReceber: TFDQuery;
    procedure FormKeyPress(Sender: TObject; var Key: Char);
    procedure BitBtn1Click(Sender: TObject);
    procedure Edit1Exit(Sender: TObject);
    procedure Edit2Exit(Sender: TObject);
    procedure BitBtn2Click(Sender: TObject);
    procedure FormKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure BitBtn4Click(Sender: TObject);
    procedure ExcluirItem1Click(Sender: TObject);
    procedure Edit3Exit(Sender: TObject);
    procedure BitBtn3Click(Sender: TObject);
    procedure Timer1Timer(Sender: TObject);
    procedure ComboBox1Exit(Sender: TObject);
    procedure BitBtn5Click(Sender: TObject);
    procedure Edit5Exit(Sender: TObject);
    procedure Timer2Timer(Sender: TObject);
    procedure BitBtn6Click(Sender: TObject);
    procedure AlteraValorUnitrio1Click(Sender: TObject);
    procedure Button1Click(Sender: TObject);
    procedure Edit8Exit(Sender: TObject);
    procedure Button2Click(Sender: TObject);
    procedure MaskEdit1Exit(Sender: TObject);
    procedure AlterarValoraMenor1Click(Sender: TObject);
    procedure Button3Click(Sender: TObject);
    procedure Button4Click(Sender: TObject);
    procedure BitBtn7Click(Sender: TObject);
    procedure BitBtn8Click(Sender: TObject);
    procedure BitBtn9Click(Sender: TObject);
    procedure BitBtn10Click(Sender: TObject);
    procedure Edit2Enter(Sender: TObject);
    procedure Button5Click(Sender: TObject);
    procedure Edit11Exit(Sender: TObject);
    procedure BitBtn11Click(Sender: TObject);
    procedure RadioButton2Click(Sender: TObject);
    procedure DBGrid1DrawColumnCell(Sender: TObject; const Rect: TRect; DataCol: Integer; Column: TColumn; State: TGridDrawState);
    procedure FormShow(Sender: TObject);
    procedure Button6Click(Sender: TObject);
    procedure BitBtn13Click(Sender: TObject);
    procedure Edit19Exit(Sender: TObject);
    procedure Edit20Exit(Sender: TObject);
    procedure BitBtn14Click(Sender: TObject);
    procedure BitBtn16Click(Sender: TObject);
    procedure Edit21Exit(Sender: TObject);
    procedure BitBtn15Click(Sender: TObject);
    procedure Edit3KeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    function TransmitirCupom: Boolean;
    procedure actModoEmissaoExecute(Sender: TObject);
    procedure imgImpClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure listMetodoPagtoKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure ComboBox1KeyPress(Sender: TObject; var Key: Char);
    procedure cdsPagtoCalcFields(DataSet: TDataSet);
    procedure ACBrBAL1LePeso(Peso: Double; Resposta: AnsiString);
    function SatAtivo: Boolean;
    procedure Edit2KeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure selecionar_item;
    procedure DBGrid1Exit(Sender: TObject);
    procedure FormResize(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure CarregarVenda(comanda:integer);
    procedure actBuscarComandaExecute(Sender: TObject);
    procedure actSalvarComandaExecute(Sender: TObject);
  private
    Findex : Integer;
    a, b: Word;
    index: Integer;
    PathArqDFe: string;
    PathPDF: string;
    PathArquivos: string;
    PathSchemas: string;
    PathTmp: string;
    vUsaGaveta, vImprime, vContingencia, voffline, vFazEntrega: Boolean;
    FTransmitirDocumento: Boolean;
    FCPF_CNPJ: String;
    FNOME_RAZAO: string;
    RecebimentoParam: TRecebimentoParam;
    vProduto: real;
    FDesconto: Double;
    procedure InputBoxSetPasswordChar(var Msg: TMessage); message InputBoxMessage;
    procedure Salvar;
    function RemoveZeros(S: string): string;
    function TirarZeros(vsParam: string): string;
    procedure alt_venc;
    procedure analisa_fecha;
    procedure baixa_estoque;
    procedure balanca;
    procedure botaoF5;
    procedure busca_codigo;
    procedure busca_emitente;
    procedure busca_movimentacao;
    procedure busca_produto;
    // procedure caixa;
    procedure Caixa(AValorCaixa: Double; AMoeda: Integer; Recebido: Boolean);
    procedure GravarRecebimento(ACodFormaPagto, AMoeda: Integer; AValor: Double);
    procedure AbreFormaPagamento;
    procedure SelecionaMetodoPagto;
    Procedure ShowHidePanel(Sender: TPanel; Isvisible: Boolean);
    function Prazo: Boolean; // #
    procedure cancela;
    procedure cancelado;
    procedure chama_mesa;
    procedure chama_temp;
    procedure consulta1;
    procedure consulta2;
    procedure contamovimentacao;
    procedure corbalcao;
    procedure devolucao;
    procedure finaliza_mesa;
    procedure gravar;
    procedure imprimir_40;
    procedure imprimir_40_new;
    procedure imprimir_pedido;
    procedure limpa;
    procedure limpa_mesa;
    procedure mov_cliente;
    procedure mov_empresa;
    procedure mov_totalizacao;
    procedure mov_usuario;
    procedure movimentacao;
    procedure muda_preco;
    procedure pagarereceber;
    procedure prazo_01;
    procedure receber_parc;
    procedure s_itevendas;
    procedure s_vendas;
    procedure s_vendas_comanda;
    procedure salva_temp;
    procedure salva_Itens_temp;
    function GetLocalIP: string;
    procedure erro_produto;
    procedure EstornaVenda;

    // Rotinas SAT
    procedure ImportaPedido;
    procedure ConfiguraSAT;
    procedure DiretoriosDeArquivos;
    function PathApp: String;
    function PathLog: String;
    function StrToPaginaCodigo(const AValor: String): TACBrPosPaginaCodigo;
    procedure GerarCFe(const ASerie, ANumero: Integer);
    procedure EnviarCFe(const ASerie, ANumero: Integer);
    procedure ConfiguraImpressora(Tipo: String);
    function GravarCFe(const ASerie, ANumero, ASerieSat, ANumeroSat: Integer; const AChave, ANumeroProtocolo: String; const ADataHoraRecto: TDateTime; const AXML: String): Boolean;
    procedure SolicitaCPF_CNPJ_CUPOM;
    procedure Limite;
    procedure ChamaLimite;
    procedure ProcAlteraValor(Sender: TComponent; AResult: Integer);
    procedure imprimir_np;
    procedure altera_atacado;
    function StrIsFloat(const S: string): Boolean;
    procedure Imprimir_40_New_01;
    procedure SetDefaultPrinter(PrinterName: String);
    function trocaImpressoraPadrao(pNomeNovaImpressora: string): Boolean;
    procedure trocaimpressora;
    function GetDefaultPrinterName: string;
    procedure itens_zerado;
    procedure ConfiguraSAT_01;
    procedure valepresente;
    procedure AjustaACBrSAT;
    procedure Caixa_Troca(AValorCaixa: Double; AMoeda: Integer;
      Recebido: Boolean);
    procedure SolicitaNOME_RAZAO_CUPOM;
    procedure altera_varejo;
    function LimparDescricao(const descricao: string): string;
    function NomeComputadorApi: String;

    //NFCe
    procedure GerarNFCe(const ASerie, ANumero: Integer);
    procedure sTransmitida;
    procedure sCancelada;
    procedure sDenegada;
    procedure sDuplicidade;
    procedure ConfiguraNFCe;
    procedure VerificarArquivo(const CaminhoArquivo: string);
    procedure Imprimir_Cupom_Compacto;
    function Getcomputer: String;

    type
      TTipoDocumento = (tdNFCe, tdCFe, tdOutro);


    { Private declarations }
  public
    TipoDocAtual: TTipoDocumento;
    valor_recebido: real;
    bal, import, canc, preco, fim, atacado: string[1];
    pvcto, vcto1: tdate;
    troco, valpago, valpagar, Especie: string;
    obs, porta: string[4];
    mesa, id, numero, cod_emi: Integer;
    cod, val1, val2, valor, pesof: string;
    valtot, rdesc, vtotal, valpeso: real;
    result, acresc: real;
    rVALOR, rTROCA, rSUBT, rDESCO, rAPAGAR, rPAGO, rTROCO, rDESCP, RVALE, RACRESCIMO, rValorPago: real;
    FMetodoPagto: string;
    vtaxa: Double;
    FFinalizado: string;
    FNota: string;
    finaliza: string;
    FValor_Total: Double;
    selecao_qtd, selecao_valor: real;
    vpNome, vpTelefone: string; //vale presente
    Fbloqueia_cliente: Boolean;
    procedure busca_cliente;
    procedure novo;
    procedure atualiza_importa;
    procedure GetsignAC(var Chave: AnsiString);
    procedure GetcodigoDeAtivacao(var Chave: AnsiString);
    procedure atualiza;
    { Public declarations }
  end;

var
  Fbalcao: TFbalcao;
  nota: string[6];
  new, tipo_balcao: string[1];
  contador: Integer;
  seq: string[6];
  vcto: tdate;
  desconto, desconto1, val_tot, Peso: real;
  quantidade: string[10];
  pesavel_kg: Integer;
  pacote: string[1];
  limite_ultrapassado: Boolean;
  xdescricao, xncm: string;
  MaskFloat: string;
  impressoraPadraoAntiga: string;
  LOGO_BALCAO_STATUS: Boolean;


implementation

uses F_buscli, F_buspro, F_parc, F_reimp, F_mesas, F_receb,
  F_admin, F_rel, F_dev, F_transacao, F_cancel, F_imp, F_dm1, F_dm,
  F_atuestoque, F_menu, F_login, F_buscli2, F_importa_comanda, Unit1, F_parcelamento,
  ufrmLimite, f_imp1, F_EdicaoItens, ufrmMovCaixa, ufrmCodigoBarras,
  ufrmFechaDiario, uSat, F_gera_etiqueta, ufrmSelecionarItem, uParametro,
  F_DescontoTotal, ufrmPreencheValePresente, uFrmAtivaDev, uFrmFrancionamento;

{$R *.dfm}

procedure TFbalcao.InputBoxSetPasswordChar(var Msg: TMessage);
var
  hInputForm, hEdit: HWND;
begin
  hInputForm := Screen.Forms[0].Handle;
  if (hInputForm <> 0) then
  begin
    hEdit := FindWindowEx(hInputForm, 0, 'TEdit', nil);
    SendMessage(hEdit, EM_SETPASSWORDCHAR, Ord('*'), 0);
  end;
end;

procedure TFbalcao.balanca;
var
  portacom: string;
begin
  { fbal := TFbal.Create(Application);
    try
    fbal.showmodal;
    finally
    FreeAndNil(fbal);
    end;
    Fbalcao.SetFocus; }

  case StrToInt(DM1.PORBAL) of
    0:
      ACBrBAL1.Device.porta := 'COM1';
    1:
      ACBrBAL1.Device.porta := 'COM2';
    2:
      ACBrBAL1.Device.porta := 'COM3';
    3:
      ACBrBAL1.Device.porta := 'COM4';
    4:
      ACBrBAL1.Device.porta := 'COM5';
    5:
      ACBrBAL1.Device.porta := 'COM6';
    6:
      ACBrBAL1.Device.porta := 'COM7';
    7:
      ACBrBAL1.Device.porta := 'COM8';
    8:
      ACBrBAL1.Device.porta := 'COM9';
  end;

  case StrToInt(DM1.VELBAL) of
    0:
      ACBrBAL1.Device.Baud := 2400;
    1:
      ACBrBAL1.Device.Baud := 4800;
    2:
      ACBrBAL1.Device.Baud := 9600;
    3:
      ACBrBAL1.Device.Baud := 19200;
    4:
      ACBrBAL1.Device.Baud := 38400;
    5:
      ACBrBAL1.Device.Baud := 56700;
    6:
      ACBrBAL1.Device.Baud := 115200;
  end;

  ACBrBAL1.Modelo := balToledo;

  try
    ACBrBAL1.Ativar;
    ACBrBAL1.LePeso(10000);
  except
    ACBrBAL1.Ativar;
    ACBrBAL1.LePeso(10000);
  end;

end;

procedure TFbalcao.cancela;
var
  codigo: string[6];
begin
  codigo := StaticText2.Caption;
  qryExecutar.Close;
  qryExecutar.SQL.Text := 'update vendas set cancelado = :d0 where nota = :d1 and origem = :D2 ';
  qryExecutar.ParamByName('d0').AsFloat := 1;
  qryExecutar.ParamByName('d1').AsInteger := StrToInt(StaticText2.Caption);
  qryExecutar.ParamByName('d2').AsString := 'BA';
  qryExecutar.ExecSQL;

  limpa;
  pnlCaixaFechado.Visible := true;
  // Panel7.Visible := true;
  Panel5.Visible := false;
  new := '0';
  ShowMessage('Nota CANCELADA Nº: ' + AjustaStr_zero_esq(codigo, 6));
end;

procedure TFbalcao.contamovimentacao;
var
  cod_id: Integer;
begin
  DM1.Q7.Close;
  DM1.Q7.SQL.Text := 'select max(id)as id from contamovimentacao';
  DM1.Q7.open;

  if DM1.Q7.FieldByName('id').IsNull then
  begin
    cod_id := 1;
  end
  else
  begin
    cod_id := DM1.Q7.FieldByName('id').AsInteger + 1;
  end;

  qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.add('insert into contamovimentacao (ativo, datainclusao, datahora, datahoracompetencia, historico, descricao, origem, credito, debito, centrocustoid, planocontaid, pessoaid, contaid, id) values (:d0,:d1,:d2,:d3,:d4,:d5,:d6,:d7,:d8,:d9,:d10,');
  qryExecutar.SQL.add(':d11,:d12,:d13)');
  qryExecutar.Params[0].AsInteger := 1;
  qryExecutar.Params[1].AsDateTime := now;
  qryExecutar.Params[2].AsDateTime := now;
  qryExecutar.Params[3].AsDateTime := now;
  qryExecutar.Params[4].AsString := 'Recebimento de Doc.: Gerado pelo sistema - Faturamento número: ' + StaticText2.Caption + 'B à vista- gerada pela venda.';
  qryExecutar.Params[5].AsString := 'Gerado pelo sistema - Faturamento número ' + StaticText2.Caption + 'B';
  qryExecutar.Params[6].AsString := 'Receber';
  qryExecutar.Params[7].AsFloat := rAPAGAR;
  qryExecutar.Params[8].AsFloat := 0;
  qryExecutar.Params[9].AsInteger := 1;
  qryExecutar.Params[10].AsInteger := 1;
  qryExecutar.Params[11].AsInteger := 1;
  qryExecutar.Params[12].AsInteger := StrToInt(Edit14.Text);
  qryExecutar.Params[13].AsInteger := cod_id;
  qryExecutar.ExecSQL;
end;

procedure TFbalcao.pagarereceber;
begin
  qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.add('insert into pagarereceber (ativo,datainclusao, numero, ordem , historico, descricao, datahora, valor, vencimento, tipo, situacao, estornado, pessoaid, especiepagamentoid, emitenteid, colaboradorid, contaid, centrocustoid, planocontaid');
  qryExecutar.SQL.add(',historicopagarereceber, boletoid, movimentacaoid) values (:d0, :D1,:d2,:d3,:d4,:d5,:d6,:d7,:d8,:d10,:d11,:d12,:d13,:d14,:d15,:d16,:d17,:d18,:d19,:d20,:d21)');
  qryExecutar.Params[0].AsInteger := 1;
  qryExecutar.Params[1].AsDateTime := now;
  qryExecutar.Params[2].AsInteger := StrToInt(StaticText2.Caption);
  qryExecutar.Params[3].AsInteger := 1;
  qryExecutar.Params[4].AsString := 'Gerado pelo Sistema - Faturamento número: ' + StaticText2.Caption + 'B';
  qryExecutar.Params[5].AsString := 'Gerado pelo Sistema - Faturamento número: ' + StaticText2.Caption + 'B';
  qryExecutar.Params[6].AsDateTime := now;
  qryExecutar.Params[7].AsFloat := rAPAGAR;
  qryExecutar.Params[8].AsDate := now;
  qryExecutar.Params[9].AsString := 'Receber';
  qryExecutar.Params[10].AsString := 'Pendentes';
  qryExecutar.Params[11].AsInteger := 0;
  qryExecutar.Params[12].AsInteger := StrToInt(Edit14.Text);
  qryExecutar.Params[13].AsInteger := 1;
  qryExecutar.Params[14].AsInteger := cod_emi;
  qryExecutar.Params[15].AsInteger := 2;
  qryExecutar.Params[16].AsInteger := 1;
  qryExecutar.Params[17].AsInteger := 1;
  qryExecutar.Params[18].AsInteger := 1;
  qryExecutar.Params[19].Clear;
  qryExecutar.Params[20].Clear;
  qryExecutar.Params[21].Clear;
  qryExecutar.ExecSQL;
end;

procedure TFbalcao.alt_venc;
begin
  Panel13.Visible := true;
  Edit17.SetFocus;
  Edit17.SetFocus;
end;

procedure TFbalcao.limpa;
begin
  StaticText2.Caption := '000000';
  edtComanda.Clear;
  edtMesa.Clear;
  ComboBox1.ItemIndex := -1;
  Edit1.Clear;
  Edit2.Clear;
  Label9.Caption := '';
  lblConsumidor.Caption := '';
  Label11.Caption := '';
  Label12.Caption := '';
  Label14.Caption := '0,0000';
  Label16.Caption := '0,00';
  Label51.Caption := '0,00';
  Edit7.Clear;
  Edit8.Clear;
  Edit9.Clear;
  Edit10.Clear;
  Edit11.Clear;
  Edit12.Clear;
  Edit13.Clear;
  Edit14.Clear;
  Edit15.Clear;
  RecebimentoParam.Clear;
  listMetodoPagto.ItemIndex := -1;
  FCPF_CNPJ := '';
  FNOME_RAZAO := '';
end;

function TFbalcao.RemoveZeros(S: string): string;
var
  I, J: Integer;
begin
  I := Length(S);
  while (I > 0) and (S[I] <= ' ') do
  begin
    Dec(I);
  end;
  J := 1;
  while (J < I) and ((S[J] <= ' ') or (S[J] = '0')) do
  begin
    Inc(J);
  end;
  result := Copy(S, J, (I - J) + 1);
end;

procedure TFbalcao.FormKeyPress(Sender: TObject; var Key: Char);
begin
  if Key = #13 then
  begin
    Key := #0;
    SelectNext(ActiveControl, true, true);
  end;
  Key := AnsiUpperCase(Key)[Length(Key)];
  if Key = #27 then
    analisa_fecha;
end;

procedure TFbalcao.FormResize(Sender: TObject);
begin
  Image2.left := (Self.ClientWidth - Image2.Width) - 10;
  Image2.Top := 10;
end;

procedure TFbalcao.analisa_fecha;
begin
  if new = '1' then
    BitBtn6.Click
  else
    Close;
//  if GUsuario.Tipo = '1' then
//    Application.Terminate;
end;

procedure TFbalcao.BitBtn1Click(Sender: TObject);
begin
  if BitBtn1.Enabled = false then
  begin
    ShowMessage('TRANSAÇÃO EM ANDAMENTO, SALVE OU CANCELE O PEDIDO !!!');

    Edit2.SetFocus;
  end
  else
  begin
    if DM1.TRANS2.InTransaction then
      DM1.TRANS2.Commit;

    DM1.TRANS2.StartTransaction;

    Edit2.Enabled := true;

    novo;

    import := '0';

    canc := '0';

    fim := '2';

    tipo_balcao := '0';

    if Parametro.VENDEDOR then
      Edit3.Enabled := true
    else
      begin
        Edit3.Enabled := false;
        Edit3.Text := '0';
      end;

    if Parametro.VENDEDOR then
    begin
      Edit3.Clear;
      if Edit3.CanFocus then
        Edit3.SetFocus;
    end
    else
    begin
      if Edit2.CanFocus then
        Edit2.SetFocus;
    end;
  end;
  limite_ultrapassado := false;
  rValorPago := 0;
  rdesc := 0;
  rDESCO := 0;
  rTROCO := 0;
  rSUBT := 0;
  vtaxa := 0;
  RACRESCIMO := 0;
  Fbloqueia_cliente := false;

  // selecao itens
  codigo_produto := 0;
  selecao_qtd := 0;
  selecao_valor := 0;
  new := '1';
  vpNome := '';
  vpTelefone := '';
  lb_CpfCnpj.Caption := '';
end;

procedure TFbalcao.atualiza_importa;
begin
  qryTMPItens.Close;
  qryTMPItens.SQL.Text := 'select * from frente_tmpitvendas where tipo = 1 order by cupom,item';
  qryTMPItens.open;
end;

procedure TFbalcao.busca_cliente;
begin
  if not Parametro.PRODUTO then
  begin
    fbuscli := Tfbuscli.Create(Application);
    try
      fbuscli.sit := '1';
      fbuscli.showmodal;
    except
       FreeAndNil(fbuscli);
    end;

    if lblConsumidor.Caption = 'Cliente Consumidor' then
    begin
      Fbalcao.ComboBox1.ItemIndex := 0;
      Edit1.SetFocus;
    end
    else
    begin
      Fbalcao.ComboBox1.SetFocus;
    end;
  end
  else
  begin
    fbuscli2 := Tfbuscli2.Create(Application);
      try
        fbuscli2.sit := '18';
        fbuscli2.showmodal;
      except
        FreeAndNil(fbuscli2);
      end;

    fbuscli2.Release;
    fbuscli2 := nil;
    if lblConsumidor.Caption = 'Cliente Consumidor' then
    begin
      Fbalcao.ComboBox1.ItemIndex := 0;
      Edit1.SetFocus;
    end
    else
    begin
      Fbalcao.ComboBox1.ItemIndex := 2;
      if Edit2.CanFocus then edit2.SetFocus;

      if Parametro.BLOQ_LIMITE then
        Limite;

      if Fbloqueia_cliente = true then
        begin
          MessageDlg('CLIENTE BLOQUEADO, FAVOR ENTRAR EM CONTATO COM O FINANCEIRO !!!',TMsgDlgType.mtError,[mbOk],0);
          Label9.Caption := '0000';
          lblConsumidor.Caption := 'Consumidor final';
          ComboBox1.ItemIndex := 0;
          lb_CpfCnpj.Caption := '';
          Exit;
        end;

      Fbalcao.ComboBox1.SetFocus;
    end;
  end;
end;

procedure TFbalcao.Edit1Exit(Sender: TObject);
begin
  // if (Edit1.Text = '') or (Edit1.Text = '0000') or (Edit1.Text = '0') or (Edit1.Text = '00') or (Edit1.Text = '000') then
  if StrToFloat(Edit1.Text) = 0 then
    Edit1.Text := '0001';

  if fim = '0' then
  begin
    // if (Edit1.Text = '') or (Edit1.Text = '0000') or (Edit1.Text = '0') or (Edit1.Text = '00') or (Edit1.Text = '000') then
    if StrToFloat(Edit1.Text) = 0 then
    begin
      ShowMessage('Digite a quantidade por favor.');
      Edit1.SetFocus;
    end
    else
    begin
      Edit1.Text := AjustaStr_zero_esq(Edit1.Text, 4);
    end;
  end;
end;

function TFbalcao.TirarZeros(vsParam: string): string;
begin
  while vsParam[1] = '0' do
    vsParam := Copy(vsParam, 2, Length(vsParam));

  result := vsParam;
end;

function TFbalcao.TransmitirCupom: Boolean;
var
  I: Integer;
begin
  try
    result := false;

    DM1.qryConfig.Close;
    DM1.qryConfig.Params[0].AsInteger := GEmitente.IDEmitente;
    DM1.qryConfig.open;

    if TipoDocAtual=tdNFCe then
      ConfiguraNFCe;

    qryVendas.Close;
    qryVendas.Params[0].AsString := StaticText2.Caption;
    qryVendas.open;

    if TipoDocAtual=tdCFe then
      ConfiguraSAT;

    DM1.ACBrNFe1.NotasFiscais.Clear;
    DM1.ACBrNFe1.Configuracoes.Geral.ModeloDF := moNFCe;

    // importa dados da venda
    ImportaPedido;

    DM1.qryNFCE_M.Close;
    DM1.qryNFCE_M.Params[0].Value := StrToInt(StaticText2.Caption); // qryVendasNOTA.Value;
    DM1.qryNFCE_M.open;

    DM1.qryNFCE_D.Close;
    DM1.qryNFCE_D.Params[0].Value := DM1.qryNFCE_MCODIGO.Value;
    DM1.qryNFCE_D.open;

    if DM1.qryNFCE_D.IsEmpty then
    begin
      ShowMessage('Itens não localizados.');
      exit;
    end;

    // Verifica atualiza cadastro de produtos
    Application.ProcessMessages;

    // Se documento padrão for NFC-e

    { // nfce
      if dm1.qryConfigTIPO_APLICATIVO.Value = 'N' then
      begin
      GerarNFCe(dm1.qryNFCE_MNUMERO.AsString);
      if not ValidaNegocios then // passo 5 valida xml
      exit;
      EnviarGravarNFCe;
      end;
    }

    DM1.qryConfig.Close;
    DM1.qryConfig.Params[0].AsInteger := GEmitente.IDEmitente;
    DM1.qryConfig.open;

    if TipoDocAtual=tdNFCe then
      GerarNFCe(DM1.qryNFCE_MSERIE.AsInteger, DM1.qryNFCE_MNUMERO.Value);


    // Se documento padrão for CF-e
    if TipoDocAtual=tdCFe then
    begin
      GerarCFe(DM1.qryNFCE_MSERIE.AsInteger, DM1.qryNFCE_MNUMERO.Value);
      EnviarCFe(DM1.qryNFCE_MSERIE.AsInteger, DM1.qryNFCE_MNUMERO.Value);
    end;
    result := true;
  except
    on e: Exception do
      raise Exception.Create(e.Message);
  end;
end;

procedure TFbalcao.Edit2Exit(Sender: TObject);
var
  ver: string[1];
  soma: real;
begin
  bal := '0';

  if Edit2.Text <> '' then
  begin
    if not Parametro.PRODUTO then
      consulta1
    else
      consulta2;
  end;
end;

procedure TFbalcao.Edit2KeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
begin
  if Key = VK_INSERT then
    exit;
end;

procedure TFbalcao.busca_codigo;
var
  lIndex: Integer;
  nComanda, nMesa : Integer;
begin
  lIndex := DM1.StartTransaction;
  try
    nota := '000001';
    seq := '1';

    qryConsulta.Close;
    qryConsulta.SQL.Text := 'SELECT GEN_ID(VENDAS_GEN, 1) AS GEN FROM RDB$DATABASE';
    qryConsulta.open;

    if not qryConsulta.IsEmpty then
    begin
      seq := qryConsulta.FieldByName('GEN').AsString;
      nota := AjustaStr_zero_esq(seq, 6);
    end;
    StaticText2.Caption := AjustaStr_zero_esq(nota, 6);

    qryExecutar.Close;
    qryExecutar.SQL.Clear;
    qryExecutar.SQL.add('insert into vendas (nota,origem,emissao,hora,saida,cfops,natureza,cliente,modelo,serie,subserie,operador,sequencia,nomepdv,n_comanda,n_mesa)');
    qryExecutar.SQL.add(' values (:d0,:d1,:d2,:d3,:d4,:d5,:d6,:d7,:d8,:d9,:d10,:D11,:D12,:nomepdv,:n_comanda,:n_mesa)');
    qryExecutar.ParamByName('d0').AsString := AjustaStr_zero_esq(StaticText2.Caption, 6);
    qryExecutar.ParamByName('d1').AsString := 'BA';
    qryExecutar.ParamByName('d2').AsDate := StrToDate(Label11.Caption);
    qryExecutar.ParamByName('d3').AsTime := StrToTime(Label12.Caption);
    qryExecutar.ParamByName('d4').AsString := 'X';
    qryExecutar.ParamByName('d5').AsString := '5.102';
    qryExecutar.ParamByName('d6').AsString := 'Venda de Mercadoria';
    qryExecutar.ParamByName('d7').AsInteger := 0;
    qryExecutar.ParamByName('d8').AsString := 'D2';
    qryExecutar.ParamByName('d9').AsString := '001';
    qryExecutar.ParamByName('d10').AsString := '01';
    qryExecutar.ParamByName('d11').AsInteger := StrToInt(Edit3.Text);
    qryExecutar.ParamByName('d12').AsInteger := StrToInt(seq);
    qryexecutar.ParamByName('nomepdv').AsString := NomeComputadorApi;


    // Nº Comanda e mesa
    if not TryStrToInt(edtComanda.Text, nComanda) then
      nComanda := 0;
    if not TryStrToInt(edtMesa.Text, nMesa) then
      nMesa := 0;
    qryexecutar.ParamByName('n_comanda').AsInteger := nComanda;
    qryexecutar.ParamByName('n_mesa').AsInteger := nMesa;

    qryExecutar.ExecSQL;
    DM1.Commit(lIndex);
  except
    on e: Exception do
    begin
      DM1.Rollback(lIndex);
      MessageDlg('Erro ao gravar Novo Codigo.' + #10#13 + 'Erro: ' + e.Message, mtError, [mbOK], 0);
    end;
  end;
end;

procedure TFbalcao.salva_temp;
var
  v1, v2, v3, v4, v5, Q1, V6, q2, q3: real;
  descricaoOriginal, descricaoLimpa: string;
begin
  qryConsulta.Close;
  qryConsulta.SQL.Clear;
  qryConsulta.SQL.add('select cupom from frente_tmpitvendas where cupom = :d0 and item = :d1 and operador = :d2 and tipo = 1');
  qryConsulta.ParamByName('d0').AsString := AjustaStr_zero_esq(nota, 6);
  qryConsulta.ParamByName('d1').AsInteger := contador;
  qryConsulta.ParamByName('d2').AsInteger := StrToInt(Edit3.Text);
  qryConsulta.open;

  if qryConsulta.IsEmpty then
  begin
    qryExecutar.Close;
    qryExecutar.SQL.Clear;
    qryExecutar.SQL.add('insert into frente_tmpitvendas (cupom,N_caixa,data,hora,operador,item,codigo,barras,descricao,qtd,preco,tributacao,icms,iss,und,desconto,acrescimo,total,serial,tipo) ');
    qryExecutar.SQL.add(' values (:d0,:d1,:d2,:d3,:d4,:d5,:d6,:d7,:d8,:d9,:d10,:d11,:d12,:d13,:d14,:d15,:d16,:d17,:d18,:tipo)');
    qryExecutar.ParamByName('d0').AsString := StaticText2.Caption;
    qryExecutar.ParamByName('d1').AsString := '1';
    qryExecutar.ParamByName('d2').AsDate := date;
    qryExecutar.ParamByName('d3').AsTime := time;
    qryExecutar.ParamByName('d4').AsInteger := StrToInt(Edit3.Text);
    qryExecutar.ParamByName('d5').AsInteger := contador;
    if not Parametro.PRODUTO then
    begin
      qryExecutar.ParamByName('d6').AsInteger := DM1.Q7.FieldByName('id').AsInteger;
      qryExecutar.ParamByName('d7').AsString := DM1.Q7.FieldByName('codigobarra').AsString;
      qryExecutar.ParamByName('d8').AsString := DM1.Q7.FieldByName('descricao').AsString;
      v1 := 0;
      v2 := 0;
      v3 := 0;
      v4 := 0;
      if bal = '1' then
      begin
        v1 := Peso;
      end
      else
      begin
        v1 := DM1.Q7.FieldByName('precovenda').AsFloat;
      end;
      v2 := (v1 / 100) * 85;
      v3 := v2 + ((v2 / 100) * 60);
      v4 := StrToFloatDef(Edit1.Text, 0);
      v5 := v1 * v4;
      q2 := 0;
      q3 := 0;
      V6 := 0;

      if bal = '1' then
      begin
        q2 := Peso;

        q3 := DM1.Q7.FieldByName('precovenda').AsFloat;

        V6 := q3 / q2;
      end;

      if bal = '1' then
      begin
        if V6 > 0 then
        begin
          qryExecutar.ParamByName('d9').AsFloat := V6;
        end
        else
        begin
          MessageDlg('QUANTIDADE NÃO PODE SER ZERO (0,00)!', mtInformation, [mbOK], 0);
          Edit2.Clear;
          Edit2.SetFocus;
          exit;
        end;
      end
      else
      begin
        if StrToFloat(Edit1.Text) > 0 then
        begin
          qryExecutar.ParamByName('d9').AsFloat := StrToFloatDef(Edit1.Text, 0);
        end
        else
        begin
          MessageDlg('QUANTIDADE NÃO PODE SER ZERO (0,00)!', mtInformation, [mbOK], 0);
          Edit2.Clear;
          Edit2.SetFocus;
          exit;
        end;
      end;

      if v1 > 0 then
      begin
        qryExecutar.ParamByName('d10').AsFloat := v1;
      end
      else
      begin
        MessageDlg('PREÇO UNITÁRIO NÃO PODE SER ZERO (0,00)!', mtInformation, [mbOK], 0);
        Edit2.Clear;
        Edit2.SetFocus;
        exit;
      end;

      qryExecutar.ParamByName('d11').AsString := '';
      qryExecutar.ParamByName('d12').AsFloat := v3;
      qryExecutar.ParamByName('d13').AsFloat := 0;
      qryExecutar.ParamByName('d14').AsString := DM1.Q7.FieldByName('sigla').AsString;
      qryExecutar.ParamByName('d15').AsFloat := 0;
      qryExecutar.ParamByName('d16').AsFloat := 0;
      qryExecutar.ParamByName('d17').AsFloat := v5;
      qryExecutar.ParamByName('d18').AsString := atacado;
      qryExecutar.ParamByName('tipo').AsInteger := 1;
      qryExecutar.ExecSQL;
    end
    else
    begin
      descricaoOriginal := Copy(qryProduto.FieldByName('descricao').AsString, 1, 75);
      descricaoLimpa := LimparDescricao(descricaoOriginal);

      qryExecutar.ParamByName('d6').AsInteger := qryProduto.FieldByName('id').AsInteger;
      qryExecutar.ParamByName('d7').AsString := qryProduto.FieldByName('codigobarra').AsString;
      qryExecutar.ParamByName('d8').AsString := descricaolimpa;  //Copy(qryProduto.FieldByName('descricao').AsString, 1, 75);
      v1 := 0;
      v2 := 0;
      v3 := 0;
      v4 := 0;
      if bal = '1' then
        v1 := Peso
      else
        v1 := qryProduto.FieldByName('precovenda').AsFloat;
      v2 := (v1 / 100) * 85;
      v3 := v2 + ((v2 / 100) * 60);
      v4 := StrToFloatDef(Edit1.Text, 0);
      v5 := v1 * v4;
      q2 := 0;
      q3 := 0;
      V6 := 0;

      if bal = '1' then
      begin
        q2 := Peso;

        q3 := qryProduto.FieldByName('precovenda').AsFloat;

        V6 := q3 / q2;
      end;

      if bal = '1' then
      begin
        if V6 > 0 then
        begin
          qryExecutar.ParamByName('d9').AsFloat := V6;
        end
        else
        begin
          MessageDlg('QUANTIDADE NÃO PODE SER ZERO (0,00)!', mtInformation, [mbOK], 0);
          Edit2.Clear;
          Edit2.SetFocus;
          exit;
        end;
      end
      else
      begin
        if StrToFloat(Edit1.Text) > 0 then
        begin
          qryExecutar.ParamByName('d9').AsFloat := StrToFloatDef(Edit1.Text, 0);
        end
        else
        begin
          MessageDlg('QUANTIDADE NÃO PODE SER ZERO (0,00)!', mtInformation, [mbOK], 0);
          Edit2.Clear;
          Edit2.SetFocus;
          exit;
        end;
      end;

      if v1 > 0 then
      begin
        qryExecutar.ParamByName('d10').AsFloat := v1;
      end
      else
      begin
        MessageDlg('PREÇO UNITÁRIO NÃO PODE SER ZERO (0,00)!', mtInformation, [mbOK], 0);
        Edit2.Clear;
        Edit2.SetFocus;
        exit;
      end;

      qryExecutar.ParamByName('d11').AsString := '';
      qryExecutar.ParamByName('d12').AsFloat := v3;
      qryExecutar.ParamByName('d13').AsFloat := 0;
      qryExecutar.ParamByName('d14').AsString := qryProduto.FieldByName('un_medida').AsString;
      qryExecutar.ParamByName('d15').AsFloat := 0;
      qryExecutar.ParamByName('d16').AsFloat := 0;
      qryExecutar.ParamByName('d17').AsFloat := v5;
      qryExecutar.ParamByName('d18').AsString := atacado;
      qryExecutar.ParamByName('tipo').AsInteger := 1;
      qryExecutar.ExecSQL;
    end;

    qryTMPItens.Close;
    qryTMPItens.SQL.Text := 'select * from frente_tmpitvendas where cupom = :d0 and tipo = 1 order by cupom, item';
    qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
    qryTMPItens.open;
    TNumericField(qryTMPItens.FieldByName('preco')).DisplayFormat := ',0.00;-,0.00';
    TNumericField(qryTMPItens.FieldByName('total')).DisplayFormat := ',0.00;-,0.00';

    contador := contador + 1;
    Edit2.SetFocus;
  end
  else
  begin
    contador := contador + 1;
    salva_temp;
    Edit2.SetFocus;
  end
end;

procedure TFbalcao.atualiza;
begin
  qryTMPItens.Close;
  qryTMPItens.SQL.Text := 'select sum(total)as valor,sum(qtd)as qtd from frente_tmpitvendas where cupom = :D0 and tipo = 1';
  qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
  qryTMPItens.open;

  // *****
  Label16.Caption := FormatFloat('0.00', qryTMPItens.FieldByName('valor').AsFloat);
  Label51.Caption := FormatFloat('0.00', qryTMPItens.FieldByName('valor').AsFloat);
  // Label14.Caption := AjustaStr_zero_esq(qryTMPItens.FieldByName('qtd').AsString, 4);
  Label14.Caption := FormatFloat('0.0000', qryTMPItens.FieldByName('qtd').AsFloat);

  qryTMPItens.Close;
  qryTMPItens.SQL.Text := 'select * from frente_tmpitvendas where cupom = :d0 and tipo = 1 order by cupom, item';
  qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
  qryTMPItens.open;
  qryTMPItens.Last;
  TNumericField(qryTMPItens.FieldByName('preco')).DisplayFormat := ',0.00;-,0.00';
  TNumericField(qryTMPItens.FieldByName('total')).DisplayFormat := ',0.00;-,0.00';

  if Edit2.CanFocus then edit2.SetFocus;
end;

procedure TFbalcao.s_vendas;
var
  nComanda,nMesa:Integer;
begin
  qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.add('update vendas set cliente=:d0,data_saida=:d1,hora_saida=:d2,formas_pgto=:d3,tot_produtos=:d4,total=:d5,operador=:d6,sequencia=:d7,');
  qryExecutar.SQL.add('       avista=:d8,modelo=:d9,serie=:d10,subserie=:d11,desconto=:d12,acrescimo=:d13,especie=:d14,loja=:d15,vale=:d16, dinheiro=:d17,');
  qryExecutar.SQL.add('       cheque=:d18, cartao=:d19, boleto=:d20, troco=:d21, quantidade=:d22,lancado=:d23, vendedor = :d24, caixa=:d25, n_comanda=:n_comanda,n_mesa=:n_mesa where nota = :d26 and origem = :d27');
  qryExecutar.ParamByName('d0').AsInteger := StrToInt(Label9.Caption);
  qryExecutar.ParamByName('d1').AsDate := StrToDate(Label11.Caption);
  qryExecutar.ParamByName('d2').AsTime := StrToTime(Label12.Caption);
  qryExecutar.ParamByName('d3').AsString := ComboBox1.Text;
  qryExecutar.ParamByName('d4').AsFloat := (StrToFloatDef(Label16.Caption, 0) + + rDESCO - RACRESCIMO );
  if ComboBox1.ItemIndex = 2 then
    qryExecutar.ParamByName('d5').AsFloat := StrToFloatDef(Label16.Caption, 0)
  else
    qryExecutar.ParamByName('d5').AsFloat := StrToFloatDef(Label16.Caption, 0); //rSUBT; // rAPAGAR;
  qryExecutar.ParamByName('d6').AsInteger := GUsuario.Operador; //  StrToInt(Edit3.Text);  //
  qryExecutar.ParamByName('d7').AsString := seq;
  qryExecutar.ParamByName('d8').AsString := '1';
  qryExecutar.ParamByName('d9').AsString := 'D2';
  qryExecutar.ParamByName('d10').AsString := '001';
  qryExecutar.ParamByName('d11').AsString := '01';
  qryExecutar.ParamByName('d12').AsFloat := rDESCO;
  qryExecutar.ParamByName('d13').AsFloat := RACRESCIMO; //acresc;
  if ComboBox1.Text = 'Á VISTA' then
    qryExecutar.ParamByName('d14').AsString := Especie
  else
    qryExecutar.ParamByName('d14').AsString := 'CARTEIRA';
  qryExecutar.ParamByName('d15').AsString := atacado;
  qryExecutar.ParamByName('d16').AsFloat := RVALE;

  if Especie = 'DINHEIRO' then
  begin
    qryExecutar.ParamByName('d17').AsFloat := StrToFloatDef(FloatToStr(rSUBT), 0);
  end
  else
  begin
    if Especie = 'CHEQUE' then
    begin
      qryExecutar.ParamByName('d18').AsFloat := StrToFloatDef(FloatToStr(rSUBT), 0);
    end
    else
    begin
      if (Especie = 'CARTAO') or (Especie = 'DEBITO') or (Especie = 'CREDITO') then
      begin
        qryExecutar.ParamByName('d19').AsFloat := StrToFloatDef(FloatToStr(rSUBT), 0);
      end
      else
      begin
        if Especie = 'BOLETO' then
        begin
          qryExecutar.ParamByName('d20').AsFloat := StrToFloatDef(FloatToStr(rSUBT), 0);
        end;
      end;
    end;
  end;

  qryExecutar.ParamByName('d21').AsFloat := rTROCO;
  qryExecutar.ParamByName('d22').AsString := Label14.Caption;
  qryExecutar.ParamByName('d23').AsString := 'EFETIVADO';

  if Parametro.VENDEDOR then
  qryExecutar.ParamByName('d24').AsInteger := StrToIntDef(Edit3.Text, 0) else
  qryExecutar.ParamByName('d24').AsInteger := 0;

  qryExecutar.ParamByName('d25').AsInteger := StrToInt(DM1.TERMINAL);
  qryExecutar.ParamByName('d26').AsString := AjustaStr_zero_esq(StaticText2.Caption, 6);
  qryExecutar.ParamByName('d27').AsString := 'BA';


  // Nº Comanda e mesa
  if not TryStrToInt(edtComanda.Text, nComanda) then
    nComanda := 0;
  if not TryStrToInt(edtMesa.Text, nMesa) then
    nMesa := 0;
  qryexecutar.ParamByName('n_comanda').AsInteger := nComanda;
  qryexecutar.ParamByName('n_mesa').AsInteger := nMesa;


  qryExecutar.ExecSQL;
end;

procedure TFbalcao.s_vendas_comanda;
var
  nComanda,nMesa:Integer;
begin
  qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.add('update vendas set cliente=:d0,data_saida=:d1,hora_saida=:d2,formas_pgto=:d3,tot_produtos=:d4,total=:d5,operador=:d6,sequencia=:d7,');
  qryExecutar.SQL.add('       avista=:d8,modelo=:d9,serie=:d10,subserie=:d11,desconto=:d12,acrescimo=:d13,especie=:d14,loja=:d15,vale=:d16, dinheiro=:d17,');
  qryExecutar.SQL.add('       cheque=:d18, cartao=:d19, boleto=:d20, troco=:d21, quantidade=:d22,lancado=:d23, vendedor = :d24, caixa=:d25, n_comanda=:n_comanda,n_mesa=:nmesa where nota = :d26 and origem = :d27');
  qryExecutar.ParamByName('d0').AsInteger := StrToInt(Label9.Caption);
  qryExecutar.ParamByName('d1').AsDate := StrToDate(Label11.Caption);
  qryExecutar.ParamByName('d2').AsTime := StrToTime(Label12.Caption);
  qryExecutar.ParamByName('d3').AsString := ComboBox1.Text;
  qryExecutar.ParamByName('d4').AsFloat := (StrToFloatDef(Label16.Caption, 0) + + rDESCO - RACRESCIMO );
  if ComboBox1.ItemIndex = 2 then
    qryExecutar.ParamByName('d5').AsFloat := StrToFloatDef(Label16.Caption, 0)
  else
    qryExecutar.ParamByName('d5').AsFloat := StrToFloatDef(Label16.Caption, 0); //rSUBT; // rAPAGAR;
  qryExecutar.ParamByName('d6').AsInteger := GUsuario.Operador; //  StrToInt(Edit3.Text);  //
  qryExecutar.ParamByName('d7').AsString := seq;
  qryExecutar.ParamByName('d8').AsString := '1';
  qryExecutar.ParamByName('d9').AsString := 'D2';
  qryExecutar.ParamByName('d10').AsString := '001';
  qryExecutar.ParamByName('d11').AsString := '01';
  qryExecutar.ParamByName('d12').AsFloat := rDESCO;
  qryExecutar.ParamByName('d13').AsFloat := RACRESCIMO; //acresc;
  if ComboBox1.Text = 'Á VISTA' then
    qryExecutar.ParamByName('d14').AsString := Especie
  else
    qryExecutar.ParamByName('d14').AsString := 'CARTEIRA';
  qryExecutar.ParamByName('d15').AsString := atacado;
  qryExecutar.ParamByName('d16').AsFloat := RVALE;

  qryExecutar.ParamByName('d17').AsFloat := 0;
  qryExecutar.ParamByName('d18').AsFloat := 0;
  qryExecutar.ParamByName('d19').AsFloat := 0;
          qryExecutar.ParamByName('d20').AsFloat := StrToFloatDef(FloatToStr(rSUBT), 0);

  qryExecutar.ParamByName('d21').AsFloat := rTROCO;
  qryExecutar.ParamByName('d22').AsString := Label14.Caption;
  qryExecutar.ParamByName('d23').AsString := 'ABERTO';

  if Parametro.VENDEDOR then
  qryExecutar.ParamByName('d24').AsInteger := StrToIntDef(Edit3.Text, 0) else
  qryExecutar.ParamByName('d24').AsInteger := 0;

  qryExecutar.ParamByName('d25').AsInteger := StrToInt(DM1.TERMINAL);
  qryExecutar.ParamByName('d26').AsString := AjustaStr_zero_esq(StaticText2.Caption, 6);
  qryExecutar.ParamByName('d27').AsString := 'BA';


  // Nº Comanda e mesa
  if not TryStrToInt(edtComanda.Text, nComanda) then
    nComanda := 0;
  if not TryStrToInt(edtMesa.Text, nMesa) then
    nMesa := 0;
  qryexecutar.ParamByName('n_comanda').AsInteger := nComanda;
  qryexecutar.ParamByName('n_mesa').AsInteger := nMesa;


  qryExecutar.ExecSQL;

end;

// #original
//procedure TFbalcao.s_itevendas;
//begin
//  qryTMPItens.Close;
//  qryTMPItens.SQL.Clear;
//  qryTMPItens.SQL.add('select * from frente_tmpitvendas where cupom = :d0 and operador = :D1 and tipo = 1 order by cupom, item');
//  qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
//  qryTMPItens.ParamByName('d1').AsInteger := StrToInt(Edit3.Text);
//  qryTMPItens.open;
//
//  qryTMPItens.First;
//
//  while not qryTMPItens.Eof do
//  begin
//    try
//      if parametro.ESTOQUE then
//        begin
//          qryBuscaQTD.Close;
//          qryBuscaQTD.SQL.Clear;
//          qryBuscaQTD.SQL.Add('select quantidade from produtoempresa where id = :id ');
//          qryBuscaQTD.ParamByName('id').AsInteger := qryTMPItens.FieldByName('codigo').AsInteger;
//          qryBuscaQTD.Open;
//
//          qryAltera.Close;
//          qryaltera.SQL.Clear;
//          qryaltera.SQL.Add(' insert into alteracao_estoque ( id , idproduto , data , hora , operador , qtd , qtd_antiga ,qtd_atual, tipo , numero) values ( :id , :idproduto , :data , :hora , :operador , :qtd , :qtd_antiga , :qtd_atual, :tipo, :numero ) ');
//          qryAltera.ParamByName('idproduto').AsInteger := qryTMPItens.FieldByName('codigo').AsInteger;
//          qryAltera.ParamByName('data').AsDate := now;
//          qryAltera.ParamByName('hora').Astime := now;
//          qryAltera.ParamByName('operador').Asstring := GUsuario.Nome;
//          qryAltera.ParamByName('qtd').AsFloat := qryTMPItens.FieldByName('qtd').AsFloat;
//          qryAltera.ParamByName('qtd_antiga').AsFloat := qryBuscaQTD.FieldByName('quantidade').AsFloat;
//          qryAltera.ParamByName('qtd_atual').AsFloat := qryBuscaQTD.FieldByName('quantidade').AsFloat - qryTMPItens.FieldByName('qtd').AsFloat;
//          qryAltera.ParamByName('TIPO').Asstring := 'BALCAO';
//          qryAltera.ParamByName('numero').Asstring := StaticText2.Caption;
//          qryaltera.ExecSQL;
//        end;
//
//      qryExecutar.Close;
//      qryExecutar.SQL.Clear;
//      qryExecutar.SQL.add('insert into itevendas (nota,origem,emissao,item,codigo,barras,cfop,st,und,qtd,preco,desconto,acrescimo,total,cancelado,sequencia,modelo,serie,subserie,serial,icms,preco_custo,SINALM)');
//      qryExecutar.SQL.add(' values (:d0,:D1,:d2,:D3,:D4,:d5,:d6,:d7,:d8,:d9,:d10,:d11,:d12,:d13,:d14,:D15,:D16,:d17,:d18,:d19,:d20,:d21,:SINALM)');
//      qryExecutar.ParamByName('d0').AsString := StaticText2.Caption;
//      qryExecutar.ParamByName('d1').AsString := 'BA';
//      qryExecutar.ParamByName('d2').AsDate := StrToDate(Label11.Caption);
//      qryExecutar.ParamByName('d3').AsInteger := qryTMPItens.FieldByName('item').AsInteger;
//      qryExecutar.ParamByName('d4').AsInteger := qryTMPItens.FieldByName('codigo').AsInteger;
//      qryExecutar.ParamByName('d5').Clear;
//      qryExecutar.ParamByName('d5').AsString := qryTMPItens.FieldByName('barras').AsString;
//      qryExecutar.ParamByName('d6').AsInteger := 5102;
//      qryExecutar.ParamByName('d7').AsString := '0000';
//      qryExecutar.ParamByName('d8').AsString := qryTMPItens.FieldByName('und').AsString;
//      qryExecutar.ParamByName('d9').AsFloat := qryTMPItens.FieldByName('qtd').AsFloat;
//      qryExecutar.ParamByName('d10').AsFloat := qryTMPItens.FieldByName('preco').AsFloat;
//      qryExecutar.ParamByName('d11').AsFloat := qryTMPItens.FieldByName('desconto').AsFloat + qryTMPItens.FieldByName('desconto1').AsFloat;
//      qryExecutar.ParamByName('d12').AsFloat := qryTMPItens.FieldByName('acrescimo').AsFloat;
//      qryExecutar.ParamByName('d13').AsFloat := qryTMPItens.FieldByName('total').AsFloat;
//      qryExecutar.ParamByName('d14').AsString := '0';
//      qryExecutar.ParamByName('d15').AsInteger := StrToIntDef(StaticText2.Caption, 0);
//      qryExecutar.ParamByName('d16').AsString := '';
//      qryExecutar.ParamByName('d17').AsString := '';
//      qryExecutar.ParamByName('d18').AsString := '';
//      if Parametro.Tipo = 2 then
//        qryExecutar.ParamByName('d19').AsString := qryTMPItens.FieldByName('serial').AsString
//      else
//        qryExecutar.ParamByName('d19').AsString := '';
//      qryExecutar.ParamByName('d20').AsFloat := qryTMPItens.FieldByName('icms').AsFloat;
//      qryExecutar.ParamByName('d21').AsFloat := qryTMPItens.FieldByName('precocusto').AsFloat;
//
//      if parametro.ESTOQUE then
//        qryExecutar.ParamByName('sinalm').AsInteger := -1 else
//        qryExecutar.ParamByName('sinalm').AsInteger := 0;
//
//      if imgdoc.Visible = true then
//        begin
//          if parametro.FISCAL_BAIXA then
//            qryExecutar.ParamByName('sinalm').AsInteger := -1 else
//            qryExecutar.ParamByName('sinalm').AsInteger := 0;
//        end;
//
//  //      end;
//  //        if imgdoc.Visible = true then
//  //          begin
//  //            if parametro.FISCAL_BAIXA then
//  //              qryExecutar.ParamByName('sinalm').AsInteger := -1 else
//  //              qryExecutar.ParamByName('sinalm').AsInteger := 0;
//  //          end
//  //          else
//  //          begin
//  //            qryExecutar.ParamByName('sinalm').AsInteger := 0
//  //
//  //
//  //        if parametro.FISCAL_BAIXA then
//  //        qryExecutar.ParamByName('sinalm').AsInteger := -1
//  //        else
//  //        qryExecutar.ParamByName('sinalm').AsInteger := 0;
//  //      end
//  //      else
//  //      begin
//  //        qryExecutar.ParamByName('sinalm').AsInteger := 0;
//  //      end;
////      qryExecutar.Transaction := dm1.GetTransaction(findex);
//      qryExecutar.ExecSQL;
//
//
//       //alterado em 06/12 David
////      qryExecutar.Close;
////      qryexecutar.sql.Clear;
////      qryExecutar.SQL.add( 'update produtoempresa set dataultimavenda = :data1 where id = :id ' );
////      qryExecutar.ParamByName('data1').AsDate := now;
////      qryExecutar.ParamByName('id').AsInteger := qryTMPItens.FieldByName('codigo').AsInteger;
////      qryExecutar.ExecSQL;
//
//      qryTMPItens.Next;
//    except
//      on E: exception do
//      begin
//        MessageDlg('Erro ao S_ITEVENDAS.' + #10#13 + 'Erro: ' + E.Message, mtError, [mbOK], 0);
//      end;
//    end;
//  end;
//end;


 procedure TFbalcao.s_itevendas;
var
  contador: Integer;
  qryVerificaDuplicidade : TIBQuery;
begin

  qryVerificaDuplicidade := TIBQuery.Create(nil);
  qryVerificaDuplicidade.Database := dm1.Data2;

  // Iniciar transação
  if not dm1.TRANS2.InTransaction then
    dm1.TRANS2.StartTransaction;

  try
    // Inicializa o contador com o próximo valor disponível para o item
    qryConsulta.Close;
    qryConsulta.SQL.Clear;
    qryConsulta.SQL.Add('SELECT COALESCE(MAX(item), 0) + 1 AS NextItem FROM itevendas WHERE nota = :nota AND origem = :origem');
    qryConsulta.ParamByName('nota').AsString := StaticText2.Caption; // Ajuste conforme necessário
    qryConsulta.ParamByName('origem').AsString := 'BA'; // Ajuste conforme necessário
    qryConsulta.Open;

    // Define o contador para o próximo item disponível
    contador := qryConsulta.FieldByName('NextItem').AsInteger;

    // Consulta para selecionar itens temporários
    qryTMPItens.Close;
    qryTMPItens.SQL.Clear;
    qryTMPItens.SQL.Add('SELECT * FROM frente_tmpitvendas WHERE cupom = :d0 AND operador = :d1 AND tipo = 1 ORDER BY cupom, item');
    qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption; // Cupom
    qryTMPItens.ParamByName('d1').AsInteger := StrToInt(Edit3.Text); // Operador
    qryTMPItens.Open;

    // Loop para inserir itens
    while not qryTMPItens.Eof do
    begin
      // Verificar se o item já existe
      qryVerificaDuplicidade.Close;
      qryVerificaDuplicidade.SQL.Clear;
      qryVerificaDuplicidade.SQL.Add('SELECT first 1 nota FROM itevendas WHERE nota = :nota AND modelo = :modelo AND origem = :origem AND item = :item');
      qryVerificaDuplicidade.ParamByName('nota').AsString := StaticText2.Caption; // Ajuste conforme necessário
      qryVerificaDuplicidade.ParamByName('modelo').AsString := qryTMPItens.FieldByName('modelo').AsString; // Ajuste conforme necessário
      qryVerificaDuplicidade.ParamByName('origem').AsString := 'BA'; // Ajuste conforme necessário
      qryVerificaDuplicidade.ParamByName('item').AsInteger := contador;
      qryVerificaDuplicidade.Open;

      // Se já existir, incrementa o contador e pula para o próximo item
      if not qryVerificaDuplicidade.IsEmpty then
      begin
        contador := contador + 1;
        qryTMPItens.Next;
        Continue;
      end;

      // Inserção de item no itevendas
      qryExecutar.Close;
      qryExecutar.SQL.Clear;
      qryExecutar.SQL.Add('INSERT INTO itevendas (nota, modelo, serie, subserie, origem, emissao, item, codigo, barras, cfop, st, und, qtd, preco, desconto, acrescimo, total, cancelado, sequencia, preco_custo, serial, icms, sinalm)');
      qryExecutar.SQL.Add('VALUES (:nota, :modelo, :serie, :subserie, :origem, :emissao, :item, :codigo, :barras, :cfop, :st, :und, :qtd, :preco, :desconto, :acrescimo, :total, :cancelado, :sequencia, :preco_custo, :serial, :icms, :sinalm)');
      qryExecutar.ParamByName('nota').AsString := StaticText2.Caption;
      qryExecutar.ParamByName('modelo').AsString := qryTMPItens.FieldByName('modelo').AsString;
      qryExecutar.ParamByName('serie').AsString := '';
      qryExecutar.ParamByName('subserie').AsString := '';
      qryExecutar.ParamByName('origem').AsString := 'BA';
      qryExecutar.ParamByName('emissao').AsDate := StrToDate(Label11.Caption); // Data de emissão
      qryExecutar.ParamByName('item').AsInteger := contador;
      qryExecutar.ParamByName('codigo').AsInteger := qryTMPItens.FieldByName('codigo').AsInteger;
      qryExecutar.ParamByName('barras').AsString := qryTMPItens.FieldByName('barras').AsString;
      qryExecutar.ParamByName('cfop').AsInteger := 5102;
      qryExecutar.ParamByName('st').AsString := '0000';
      qryExecutar.ParamByName('und').AsString := qryTMPItens.FieldByName('und').AsString;
      qryExecutar.ParamByName('qtd').AsFloat := qryTMPItens.FieldByName('qtd').AsFloat;
      qryExecutar.ParamByName('preco').AsFloat := qryTMPItens.FieldByName('preco').AsFloat;
      qryExecutar.ParamByName('desconto').AsFloat := qryTMPItens.FieldByName('desconto').AsFloat + qryTMPItens.FieldByName('desconto1').AsFloat; // Soma de descontos
      qryExecutar.ParamByName('acrescimo').AsFloat := qryTMPItens.FieldByName('acrescimo').AsFloat;
      qryExecutar.ParamByName('total').AsFloat := qryTMPItens.FieldByName('total').AsFloat;
      qryExecutar.ParamByName('cancelado').AsString := '0';
      qryExecutar.ParamByName('sequencia').AsInteger := StrToIntDef(StaticText2.Caption, 0);
      qryExecutar.ParamByName('preco_custo').AsFloat := qryTMPItens.FieldByName('precocusto').AsFloat;

      if Parametro.Tipo = 2 then
         qryExecutar.ParamByName('serial').AsString := qryTMPItens.FieldByName('serial').AsString
      else
        qryExecutar.ParamByName('serial').AsString := '';

      qryExecutar.ParamByName('icms').AsFloat := qryTMPItens.FieldByName('icms').AsFloat;
      qryExecutar.ParamByName('sinalm').AsInteger := IfThen(parametro.ESTOQUE, -1, 0);

      qryExecutar.ExecSQL;

      // Incrementa o contador após inserção
      contador := contador + 1;
      qryTMPItens.Next;
    end;

    // Confirmar transação ao final se não houver erros
    dm1.TRANS2.Commit;
    qryVerificaDuplicidade.Free;
  except
    on E: Exception do
    begin
      // Reverter transação em caso de erro
      dm1.TRANS2.Rollback;
      qryVerificaDuplicidade.Free;
      ShowMessage('Erro ao inserir item: ' + E.Message);
    end;
  end;
end;

procedure TFbalcao.gravar;
var
  impressoraPadraoAntiga: string;
begin
  qryTMPItens.Close;
  qryTMPItens.SQL.Clear;
  qryTMPItens.SQL.add('select sum(total) from frente_tmpitvendas where cupom = :D0 and tipo = 1');
  qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
  qryTMPItens.open;

  desconto := 0;

  { if ComboBox1.ItemIndex = 0 then
    begin
    if Especie = 'DINHEIRO' then
    caixa;
    end
    else
    begin
    if ComboBox1.ItemIndex = 1 then
    begin
    prazo;
    end
    else
    begin
    if ComboBox1.ItemIndex = 2 then
    begin
    receber_parc;
    end;
    end;
    end; }


  // Multiplos pagamentos
  // AbreFormaPagamento;

  if not FTransmitirDocumento then
  begin
    if (Parametro.laser = 1) or (Parametro.laser = 5) then
    begin
      if MessageDlg('Deseja IMPRIMIR esse PEDIDO?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
        imprimir_pedido;
      if MessageDlg('Deseja 2ª Via do Pedido?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
        imprimir_pedido;
    end
    else
    begin
      if Parametro.laser = 2 then
      begin
        if MessageDlg('Deseja IMPRIMIR esse PEDIDO?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
          imprimir_40;
        if MessageDlg('Deseja 2ª Via do Pedido?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
          imprimir_40;
      end
      else
      begin
        if Parametro.LASER = 3 then
        begin
//          if MessageDlg('Deseja IMPRIMIR esse PEDIDO?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
//          begin
//            imprimir_40_new;
//          end
          if Parametro.IMPRIMIR_CUPOM then
            imprimir_40_new
          else
            iF MessageDlg('DESEJA IMPRIMIR 1ª VIA DO CUPOM?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
              imprimir_40_new;

          if Label9.Caption <> '0' then
          begin
            if Parametro.NP then
            begin
              iF MessageDlg('IMPRIMIR NOTA PROMISSÓRIA?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
                imprimir_np;
            end;
          end;

          if Parametro.VALE_PRESENTE_BALCAO then
             valepresente;
        end
        else
        begin
          if fimp = nil then
            fimp := Tfimp.Create(Application);

          RLPrinter.PrinterName := Parametro.impressora;

          fimp.RLReport10.PrintDialog := false;

          fimp.RLReport10.Print;

          fimp.Release;
          fimp := nil;
          Fbalcao.SetFocus;
        end;

        if Parametro.NVIAS then
        begin
          if MessageDlg('DESEJA IMPRIMIR 2ª VIA DO CUPOM?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
          begin
            imprimir_40_new;
          end
        end;
      end;
    end;
  end;

  qryConsulta.Close;
  qryConsulta.SQL.Clear;
  qryConsulta.SQL.add('select max(sequencia) from vendas');
  qryConsulta.open;
  qryConsulta.Last;

  seq := FloatToStr(qryConsulta.FieldByName('max').AsFloat + 1);

  if not FTransmitirDocumento then
  begin
    s_vendas;

    if Parametro.SALVA_ITEVENDAS_BALCAO then
    s_itevendas;

    //if Parametro.ESTOQUE then           alteracao estoque trigger
    //  baixa_estoque;
  end
  else
  begin
    Try
      if not PARAMETRO.NAO_GERA_PEDIDO then
      begin
        s_vendas;

        if Parametro.SALVA_ITEVENDAS_BALCAO then
        s_itevendas;
      end;

      if TransmitirCupom then
      begin

        // s_itevendas;
        // if Parametro.FISCAL_BAIXA then
        //if Parametro.FISCAL_BAIXA then           alteracao estoque trigger
        //  baixa_estoque;

      end
      else
        EstornaVenda;
    except
      on e: Exception do
        ShowMessage('Falha ao emitir CF-e.' + #13 + 'Erro: ' + e.Message);
    end;
  end;

  {qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.add('delete from frente_tmpitvendas where cupom = :d0');
  qryExecutar.ParamByName('d0').AsString := StaticText2.Caption;
  qryExecutar.ExecSQL;}

  qryTMPItens.Close;
  qryTMPItens.SQL.Clear;
  qryTMPItens.SQL.add('select * from frente_tmpitvendas where cupom = :d0 and tipo = 1 order by cupom, item');
  qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
  qryTMPItens.open;

  pnlCaixaFechado.Visible := true;
  BitBtn1.Enabled := true;
  BitBtn2.Enabled := false;
  BitBtn6.Enabled := false;

  Edit2.Enabled := false;

  new := '0';
end;

procedure TFbalcao.Salvar;
var
satok : Boolean;
begin
satok := false;

  try
    Edit1.Text := '0001';
    atualiza;
    fim := '1';

    if (not Parametro.FISCAL_BAIXA) and (imgDoc.Visible = true) then
    begin

    end
    else
    begin
      if (Parametro.CONTASARECEBER) then
      begin
        case ComboBox1.ItemIndex of
          0:
            begin
              if Parametro.DESMEMBRA then
              begin
                RecebimentoParam.nota := StaticText2.Caption;
                RecebimentoParam.Valor_Total := StrToFloatDef(Label51.Caption, 0);
                RecebimentoParam.CodigoCliente := StrToInt(Label9.Caption);
                RecebimentoParam.DataNota := StrToDate(Label11.Caption);
                RecebimentoParam.IndexReceb := 1;
                RecebimentoParam.IndexVenda := 1;
                if not FRecebimento(RecebimentoParam) then
                begin
                  // ALimpa := False;
                  exit;
                end;

                if (imgDoc.Visible = true) and (Parametro.FISCAL_BAIXA) then
                  begin
                    if (RecebimentoParam.ValorCaixa > 0) then
                      Caixa(RecebimentoParam.ValorCaixa, 0, Parametro.DESMEMBRA);

                    if (RecebimentoParam.ValorCaixa > 0) then
                      Caixa_Troca(RecebimentoParam.ValorCaixa, 0, Parametro.DESMEMBRA);
                  end
                  else
                  begin
                    if (imgDoc.Visible = false) and (Parametro.FISCAL_BAIXA ) then
                      begin
                        if (RecebimentoParam.ValorCaixa > 0) then
                          Caixa(RecebimentoParam.ValorCaixa, 0, Parametro.DESMEMBRA );

                        if (RecebimentoParam.ValorCaixa > 0) then
                          Caixa_Troca(RecebimentoParam.ValorCaixa, 0, Parametro.DESMEMBRA);
                      end
                      else
                      begin
//                        if (imgDoc.Visible = false) and ( not Parametro.FISCAL_BAIXA ) then
//                          begin
//                            if (RecebimentoParam.ValorCaixa > 0) then
//                              Caixa(RecebimentoParam.ValorCaixa, 0, Parametro.DESMEMBRA);
//                          end;
                      end;
                  end;
              end
              else
                if (imgDoc.Visible = true) and (Parametro.FISCAL_BAIXA) then
                Caixa(Total, 0, Parametro.DESMEMBRA) else
                if (imgDoc.Visible = false) and (Parametro.FISCAL_BAIXA) then
                Caixa(Total, 0, Parametro.DESMEMBRA);
//                if (imgDoc.Visible = false) and ( not Parametro.FISCAL_BAIXA ) then
//                Caixa(Total, 0, Parametro.DESMEMBRA);
            end;

          1:
            prazo_01;
          2:
            receber_parc;
        end;
      end;
    end;

    if FFinalizado = '1' then
    begin
      atualiza;
      exit;
    end;

    gravar;
    limpa;
    Edit1.Text := '0001';

    // dm1.Commit(index);
    DM1.TRANS2.Commit;
  except
    on e: Exception do
    begin
      // dm1.Rollback(index);
      DM1.TRANS2.Rollback;
      MessageDlg('Erro ao Salvar Pedido' + #10#13 + 'Erro: ' + e.Message, mtError, [mbOK], 0);
    end;
  end;
  limite_ultrapassado := false;
end;

procedure TFbalcao.BitBtn2Click(Sender: TObject);
var
  InputString: string;
begin
//  if (ComboBox1.ItemIndex <> 0) and (imgDoc.Visible = true) then
//   begin
//      MessageDlg('VERIFICAR, TRANSAÇÃO NÃO PODERÁ SER REALIZADA NESSE MEIO DE PAGAMENTO !!!', mtWarning, [mbOK], 0);
//      exit;
//   end;

  if (ComboBox1.ItemIndex <> 0) and (LABEL9.CaptioN = '0') then
   begin
      MessageDlg('VERIFICAR, FAVOR INDICAR CLIENTE PARA ESSA TRANSAÇÃO !!!', mtWarning, [mbOK], 0);
      exit;
   end;

  // if FTransmitirDocumento then
  // if not SatAtivo then
  // exit;

  itens_zerado;

  if Parametro.BLOQ_LIMITE then
    Limite;

  if limite_ultrapassado then
  begin
    PostMessage(Handle, InputBoxMessage, 0, 0);
    InputString := InputBox('Senha', 'Digite a senha', '');
    if InputString = Parametro.senhad then
      Salvar
    else
      MessageDlg('CLIENTE COM VALOR DE LIMITE ULTRAPASSADO, NÃO CONSEGUIRÁ FINALIZAR A VENDA!!!', mtWarning, [mbOK], 0);
  end
  else
  begin
    Salvar;
  end;

end;

procedure TFbalcao.chama_mesa;
begin
  if fmesas = nil then
    fmesas := Tfmesas.Create(Application);
  fmesas.showmodal;
  fmesas.Release;
  fmesas := nil;
  Fbalcao.atualiza_importa;
  Fbalcao.SetFocus;
end;

procedure TFbalcao.FormClose(Sender: TObject; var Action: TCloseAction);
var
  Trans: TIBTransaction;
begin
  Trans := DM1.GetTransaction(index);
  if (Trans <> nil) and (Trans.InTransaction) then
    Trans.Rollback;

  SetDefaultPrinter(impressoraPadraoAntiga);
  Action := CaFree;
end;

procedure TFbalcao.FormCreate(Sender: TObject);
begin
  dm1.NomeTerminal := Getcomputer;


  Self.Position := poScreenCenter; // Centraliza o formulário na tela

  // Define o TImage para um tamanho fixo
  Image2.Width := 300;   // Ajuste conforme necessário
  Image2.Height := 300;  // Ajuste conforme necessário
  // Posiciona o TImage no topo e lado direito ao carregar o formulário
  Image2.Top := 0;
  Image2.Left := Self.ClientWidth - Image2.Width; // Calcula a posição do lado direito
end;

procedure TFbalcao.FormKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
begin
  if Parametro.RESTAURANTE then
  begin
    if Key = vk_f1 then
    begin
      if fimporta_comanda = nil then
        fimporta_comanda := Tfimporta_comanda.Create(Application);
      fimporta_comanda.showmodal;
      fimporta_comanda.Release;
      fimporta_comanda := nil;
      Fbalcao.atualiza;
      Fbalcao.SetFocus;
    end;
  end;

   if Key = vk_f2 then
    devolucao;

  if new = '1' then
    begin
      if Key = vk_F3 then
        busca_cliente;
    end;

  if new = '0' then
    begin
      if Key = vk_F4 then
        BitBtn1.Click; // novo;
    end;

  if new = '1' then
    begin
      if Key = vk_F5 then
        BitBtn2.Click;
    end;

  if new = '1' then
    begin
      if Key = vk_F6 then
        BitBtn6.Click;
    end;

  if Key = vk_F7 then
    BitBtn3.Click;

  if new = '0' then
    begin
      if Key = vk_F8 then
        muda_preco;
    end;

  if not GUsuario.Visualizar then
  begin
    if Key = vk_F9 then
    begin
      if BitBtn7.Enabled = true then
        BitBtn7.Click;
    end;
  end;

  if not GUsuario.Visualizar then
  begin
    if Key = vk_F10 then
      BitBtn11.Click;
  end;

  if Key = vk_F11 then
    cancelado;
  // IF key = vk_F11 then alt_venc;

  if Key = vk_f12 then
  begin
//    Panel20.Visible := true;
//    Edit20.SetFocus;
  end;

  if Key = VK_INSERT then
  begin
    fim := '1';
    Edit1.Enabled := true;
    Edit1.Clear;
    Edit1.SetFocus;
  end;

  if Key = vk_HOME then
  begin
    Panel9.Visible := true;
    Edit7.SetFocus;
    Label25.Caption := dm.TempITEM.AsString;
    Label26.Caption := dm.TempDESCRICAO.Value;
    Edit7.Text := dm.TempQTD.AsString;
    Edit8.Text := dm.TempPRECO.AsString;
    Edit9.Text := dm.TempTOTAL.AsString;
    Label32.Caption := (FloatToStr(StrToFloatDef(Edit8.Text, 0) - (StrToFloatDef(Edit8.Text, 0) * 8) / 100));
    Label32.Caption := FormatFloat('0.00', StrToFloatDef(Label32.Caption, 0));
  end;

  if (ssCtrl in Shift) then
    if Key = 65 then
    begin
      altera_atacado;
    end;

  if (ssCtrl in Shift) then
    if Key = 66 then
    begin
      frmCodigoBarras := TfrmCodigoBarras.Create(Application);
      try
        frmCodigoBarras.showmodal;
      finally
        FreeAndNil(frmCodigoBarras);
      end;
    end;

  if (ssCtrl in Shift) then
    if Key = 68 then
    begin
      FDesconto := AbreDescontoTotal((FValor_Total), FNota);
    end;

  if (ssCtrl in Shift) then
    if Key = 70 then
    begin
      frmFechaDiario := TfrmFechaDiario.Create(Application);
      try
        frmFechaDiario.showmodal;
      finally
        FreeAndNil(frmFechaDiario);
      end;
    end;

  if (ssCtrl in Shift) then
    if Key = 71 then
    begin
      fgera_etiqueta := Tfgera_etiqueta.Create(Application);
      try
        fgera_etiqueta.showmodal;
      finally
        FreeAndNil(fgera_etiqueta);
      end;
    end;

  if (ssCtrl in Shift) then
    if Key = 78 then
    begin
      frmSAT := TfrmSAT.Create(Application);
      try
        frmSAT.showmodal;
      finally
        FreeAndNil(frmSAT);
      end;
    end;

  if (ssCtrl in Shift) then
    if Key = 83 then
    begin
      frmMovcaixa := TFrmMovCaixa.Create(Application);
      try
        frmMovcaixa.showmodal;
      finally
        FreeAndNil(frmMovcaixa);
      end;
    end;

  if (ssCtrl in Shift) then
    if Key = 84 then
    begin
    FrmAtivaDev := TFrmAtivaDev.create(Application);
      try
        FrmAtivaDev.showmodal;
      except
        FreeAndNil(FrmAtivaDev);
      end;
      atualiza;
    end;

  if (ssCtrl in Shift) then
    if Key = 85 then
    begin
      altera_varejo;
    end;

  if (ssCtrl in Shift) then       //antes estava crtl + v - key = 86
    if Key = 72 then
    begin
      FrmFrancionamento := TFrmFrancionamento.create(application);
        try
          FrmFrancionamento.showmodal;
        except
          FreeAndNil(FrmFrancionamento);
        end;
    end;
end;

procedure TFbalcao.novo;
begin
  preco := '0';

  pnlCaixaFechado.Visible := false;
  Panel6.Visible := true;
  // Panel7.Visible := false;
  Label9.Caption := '';
  lblConsumidor.Caption := '';

  Edit3.Text := IntToStr(GUsuario.Operador);

  if Parametro.Tipo = 3 then
  begin
    Label9.Caption := '0';
    lblConsumidor.Caption := 'Cliente Consumidor';
    ComboBox1.ItemIndex := 0;
    Edit1.Text := '0001';
  end
  else
  begin
    busca_cliente;

    Edit1.Text := '0001';
    Edit1.SetFocus;
  end;
  Label11.Caption := DateToStr(date);
  Label12.Caption := TimeToStr(time);

  busca_codigo;

  contador := 1;

  BitBtn1.Enabled := false;
  BitBtn2.Enabled := true;
  BitBtn6.Enabled := true;

  fim := '0';
  new := '1';
  edit2.Clear;
end;

procedure TFbalcao.BitBtn4Click(Sender: TObject);
begin
  fim := '1';
  busca_cliente;
  if Label9.Caption <> '0000' then
    begin
      if Parametro.BLOQ_LIMITE then
        Limite;

      ComboBox1.ItemIndex := 2;
    end;
end;

procedure TFbalcao.EstornaVenda;
begin
  qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.Text := 'delete from vendas where nota=:nota and origem=' + QuotedStr('BA');
  qryExecutar.ParamByName('nota').AsString := StaticText2.Caption;
  qryExecutar.ExecSQL;
end;

procedure TFbalcao.ExcluirItem1Click(Sender: TObject);
begin
  qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.add('delete from frente_tmpitvendas where item = :d0 and cupom = :D1 and tipo = 1 ');
  qryExecutar.Params[0].AsInteger := qryTMPItens.FieldByName('item').AsInteger;
  qryExecutar.Params[1].AsString := StaticText2.Caption;
  qryExecutar.ExecSQL;
  ShowMessage('Item excluído com Sucesso.');

  qryTMPItens.Close;
  qryTMPItens.SQL.Clear;
  qryTMPItens.SQL.add('select * from frente_tmpitvendas where cupom = :d0 and tipo = 1 order by cupom, item');
  qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
  qryTMPItens.open;

  atualiza;
end;

procedure TFbalcao.Edit3Exit(Sender: TObject);
begin
if new = '1' then
  begin
    if edit3.Text = '' then
      begin
        MessageDlg('Erro: Favor indicar o VENDEDOR.', mtError, [mbOK], 0);
        if EDIT3.CanFocus THEN Edit3.SetFocus;
        Exit;
      end;

      qryVendedor.Close;
      qryVendedor.SQL.Clear;
      qryVendedor.SQL.add('select * from vendedor where id = :id');
      qryVendedor.ParamByName('id').AsInteger := StrToIntDef(Edit3.Text, 0);
      qryVendedor.open;

      if qryVendedor.IsEmpty then
      begin
        MessageDlg('Erro: Vendedor não encontrado.', mtError, [mbOK], 0);
        Edit3.Clear;
        Edit3.SetFocus;
      end
      else
      begin
        StaticText5.Caption := qryVendedor.FieldByName('nome').AsString;
        if Edit2.CanFocus then
          Edit2.SetFocus;
      end;
  end;
end;

procedure TFbalcao.Edit3KeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
begin
  if Key = vk_F6 then
  begin
    exit;
  end;
end;

procedure TFbalcao.Caixa(AValorCaixa: Double; AMoeda: Integer; Recebido: Boolean);
var
  saldo: real;
  codigo: Integer;
begin
  saldo := 0;
  codigo := 0;

  qryConsulta.Close;
  qryConsulta.SQL.Text := 'select max(codigo) from caixa';
  qryConsulta.open;
  codigo := qryConsulta.FieldByName('max').AsInteger;

  qryConsulta.Close;
  qryConsulta.SQL.Text := 'select saldo from caixa where codigo = :d0';
  qryConsulta.ParamByName('d0').AsInteger := codigo;
  qryConsulta.open;

  if qryConsulta.IsEmpty then
    saldo := 0
  else
    saldo := qryConsulta.FieldByName('saldo').AsFloat;
  try
    qryExecutar.Close;
    qryExecutar.SQL.Clear;
    qryExecutar.SQL.add('INSERT INTO CAIXA (DATA,HORA,DOCUMENTO,CUSTO,CONTA, ENTRADA,SAIDA,SALDO,HISTORICO,GRAVACAO,ORIGEM,CODPROF,TERMINAL,VENDEDOR) ');
    qryExecutar.SQL.add('           VALUES (:DATA,:HORA,:DOCUMENTO,:CUSTO,:CONTA,:ENTRADA,:SAIDA,:SALDO,:HISTORICO,:GRAVACAO,:ORIGEM,:CODPROF,:TERMINAL,:VENDEDOR)');
    qryExecutar.ParamByName('DATA').AsDate := StrToDate(Label11.Caption);
    qryExecutar.ParamByName('HORA').AsTime := StrToTime(Label12.Caption);
    qryExecutar.ParamByName('DOCUMENTO').AsString := 'D' + StaticText2.Caption;
    qryExecutar.ParamByName('CUSTO').AsInteger := 0;
    qryExecutar.ParamByName('CONTA').AsInteger := 0;
    if not Parametro.DESMEMBRA then
      qryExecutar.ParamByName('ENTRADA').AsFloat := ValorValido(Label14.Caption, 0)
    else
      qryExecutar.ParamByName('ENTRADA').AsFloat := AValorCaixa; // ValorValido(label14.Caption);
    qryExecutar.ParamByName('SAIDA').AsFloat := 0;
    if not Parametro.DESMEMBRA then
      qryExecutar.ParamByName('SALDO').AsFloat := saldo + ValorValido(Label14.Caption, 0)
    else
      qryExecutar.ParamByName('SALDO').AsFloat := saldo + AValorCaixa; // ValorValido(label14.Caption);
    qryExecutar.ParamByName('HISTORICO').AsString := 'Venda à vista - fatura ' + Label9.Caption + ' ' + Copy(lblConsumidor.Caption, 1, 20);
    qryExecutar.ParamByName('GRAVACAO').AsString := Label11.Caption + '  ' + Label12.Caption;
    qryExecutar.ParamByName('ORIGEM').AsString := 'VEN' + RemoveZeros(StaticText2.Caption);
    // if not Parametro.PROFISSIONAL then
    qryExecutar.ParamByName('CODPROF').AsInteger := 0;
    qryExecutar.ParamByName('TERMINAL').AsInteger := StrToIntDef(DM1.TERMINAL, 0);
    qryExecutar.ParamByName('VENDEDOR').AsInteger := StrToIntDef(Edit3.Text, 0);
    qryExecutar.ExecSQL;

    if not Recebido then
      GravarRecebimento(0, AMoeda, AValorCaixa);

  except
    on e: Exception do
    begin
      MessageDlg('Erro : CAIXA.' + #10#13 + 'Erro: ' + e.Message, mtError, [mbOK], 0);
      raise;
    end;
  end;
end;

procedure TFbalcao.Caixa_Troca(AValorCaixa: Double; AMoeda: Integer; Recebido: Boolean);
var
  saldo: real;
  codigo: Integer;
begin
  saldo := 0;
  codigo := 0;

  qryRecebimento.Close;
  qryRecebimento.SQL.Clear;
  qryRecebimento.SQL.Add('select valor from recebimento_vendas where nota = :nota and ID_FORMA_PAGAMENTO = 7 ');
  qryRecebimento.ParamByName('nota').AsString := StaticText2.Caption;
  qryRecebimento.Open;

  if qryRecebimento.FieldByName('valor').AsFloat > 0 then
  begin
    qryConsulta.Close;
    qryConsulta.SQL.Text := 'select max(codigo) from caixa';
    qryConsulta.open;
    codigo := qryConsulta.FieldByName('max').AsInteger;

    qryConsulta.Close;
    qryConsulta.SQL.Text := 'select saldo from caixa where codigo = :d0';
    qryConsulta.ParamByName('d0').AsInteger := codigo;
    qryConsulta.open;

    if qryConsulta.IsEmpty then
      saldo := 0
    else
      saldo := qryConsulta.FieldByName('saldo').AsFloat;
    try
      qryExecutar.Close;
      qryExecutar.SQL.Clear;
      qryExecutar.SQL.add('INSERT INTO CAIXA (DATA,HORA,DOCUMENTO,CUSTO,CONTA, ENTRADA,SAIDA,SALDO,HISTORICO,GRAVACAO,ORIGEM,CODPROF,TERMINAL,VENDEDOR) ');
      qryExecutar.SQL.add('           VALUES (:DATA,:HORA,:DOCUMENTO,:CUSTO,:CONTA,:ENTRADA,:SAIDA,:SALDO,:HISTORICO,:GRAVACAO,:ORIGEM,:CODPROF,:TERMINAL,:VENDEDOR)');
      qryExecutar.ParamByName('DATA').AsDate := StrToDate(Label11.Caption);
      qryExecutar.ParamByName('HORA').AsTime := StrToTime(Label12.Caption);
      qryExecutar.ParamByName('DOCUMENTO').AsString := 'D' + StaticText2.Caption;
      qryExecutar.ParamByName('CUSTO').AsInteger := 0;
      qryExecutar.ParamByName('CONTA').AsInteger := 0;
      qryExecutar.ParamByName('ENTRADA').AsFloat := 0;//AValorCaixa; // ValorValido(label14.Caption);
      qryExecutar.ParamByName('SAIDA').AsFloat := qryRecebimento.FieldByName('valor').AsFloat;    //   ///RVALE;
      qryExecutar.ParamByName('SALDO').AsFloat := saldo - qryRecebimento.FieldByName('valor').AsFloat; //RVALE; //+ AValorCaixa; // ValorValido(label14.Caption);
      qryExecutar.ParamByName('HISTORICO').AsString := 'VALE TROCA - ' + Label9.Caption + ' ' + Copy(lblConsumidor.Caption, 1, 20);
      qryExecutar.ParamByName('GRAVACAO').AsString := Label11.Caption + '  ' + Label12.Caption;
      qryExecutar.ParamByName('ORIGEM').AsString := 'VEN' + RemoveZeros(StaticText2.Caption);
      // if not Parametro.PROFISSIONAL then
      qryExecutar.ParamByName('CODPROF').AsInteger := 0;
      qryExecutar.ParamByName('TERMINAL').AsInteger := StrToIntDef(DM1.TERMINAL, 0);
      qryExecutar.ParamByName('VENDEDOR').AsInteger := StrToIntDef(Edit3.Text, 0);
      qryExecutar.ExecSQL;

      if not Recebido then
        GravarRecebimento(0, AMoeda, AValorCaixa);

    except
      on e: Exception do
      begin
        MessageDlg('Erro : CAIXA TROCA.' + #10#13 + 'Erro: ' + e.Message, mtError, [mbOK], 0);
        raise;
      end;
    end;
  end;
end;


procedure TFbalcao.busca_produto;
begin
  fbuspro := Tfbuspro.Create(Application);
  try
    fbuspro.sit := '1';
    fbuspro.showmodal;
  except
    FreeAndNil(fbuspro);
  end;
end;

procedure TFbalcao.BitBtn3Click(Sender: TObject);
begin
  busca_produto;
end;

function TFbalcao.SatAtivo: Boolean;
var
  ModeloSAT: TSatModelo;
begin
  if Parametro.SAT then
  BEGIN
    DM1.qryConfig.Close;
    DM1.qryConfig.Params[0].AsInteger := GEmitente.IDEmitente;
    DM1.qryConfig.open;

    ModeloSAT := DM1.GetModeloSAT(DM1.qryConfigMODELO_DLL.AsString);

    DM1.AcbrSAT1.OnGetsignAC := GetsignAC;
    DM1.AcbrSAT1.OnGetcodigoDeAtivacao := GetcodigoDeAtivacao;
    DM1.AcbrSAT1.Modelo := ModeloSAT.Tipo;
    DM1.AcbrSAT1.NomeDLL := ModeloSAT.PathDll;
    DM1.AcbrSAT1.Config.ide_numeroCaixa := StrToInt(DM1.TERMINAL);
    DM1.AcbrSAT1.Config.ide_CNPJ := TiraPontos(DM1.qryConfigSAT_CNPJ.AsString);
    DM1.AcbrSAT1.Config.emit_CNPJ := TiraPontos(GEmitente.CPFCNPJ);
    DM1.AcbrSAT1.Config.emit_IE := TiraPontos(GEmitente.InscRG);
    DM1.AcbrSAT1.Config.emit_IM := TiraPontos(GEmitente.InscMunicipal);
    DM1.AcbrSAT1.Config.emit_cRegTribISSQN := RTISSMicroempresaMunicipal;
    DM1.AcbrSAT1.Config.emit_indRatISSQN := irSim;
    DM1.AcbrSAT1.Config.PaginaDeCodigo := ModeloSAT.PaginaCodigo;
    DM1.AcbrSAT1.Config.EhUTF8 := ModeloSAT.UTF8;
    DM1.AcbrSAT1.Config.infCFe_versaoDadosEnt := StrToFloatDef(DM1.qryConfigCFE_VERSAO.AsString, 0.07);

    DM1.AcbrSAT1.Inicializar;
    try
      DM1.AcbrSAT1.ConsultarStatusOperacional;

      if DM1.AcbrSAT1.Resposta.codigoDeRetorno = 10000 then
      begin
        if DM1.AcbrSAT1.Inicializado then
        begin
          result := true;
        end
        else
        begin
          result := false;
          MessageDlg('"SAT" INATIVO ou SEM COMUNICAÇÃO!', mtError, [mbOK], 0);
        end;
      end;
    except
      result := false;
      MessageDlg('"SAT" INATIVO ou SEM COMUNICAÇÃO!', mtError, [mbOK], 0);
    end;
  END;
  DM1.AcbrSAT1.DesInicializar;
end;

procedure TFbalcao.Timer1Timer(Sender: TObject);
begin
  { with Label3 do
    Visible := not Visible;
    with Label30 do
    Visible := not Visible;
    with Label31 do
    Visible := not Visible; }
end;

procedure TFbalcao.imprimir_pedido;
var
  cidade, sigla: string[40];
  tel, ende, bairro, num: string;
begin
  qryTMPItens.Close;
  qryTMPItens.SQL.Clear;
  qryTMPItens.SQL.add('select * from FRENTE_TMPITVENDAS where cupom = :D0 and tipo = 1 order by cupom, item');
  qryTMPItens.Params[0].AsString := StaticText2.Caption;
  qryTMPItens.open;

  if Label9.Caption <> '1' then
  begin
    DM1.q2.Close;
    DM1.q2.SQL.Text := 'select t.numero, e.logradouro, (e.numero)as nume, e.bairro, e.municipioid from endereco e, telefone t where t.id = e.id and t.pessoaid = :d0';
    DM1.q2.Params[0].AsInteger := StrToInt(Label9.Caption);
    DM1.q2.open;

    tel := DM1.q2.FieldByName('numero').AsString;
    ende := DM1.q2.FieldByName('logradouro').AsString;;
    num := DM1.q2.FieldByName('nume').AsString;;
    bairro := DM1.q2.FieldByName('bairro').AsString;;
  end;

  DM1.q4.Close;
  DM1.q4.SQL.Text := 'select municipioid from endereco where pessoaid = :d0';
  DM1.q4.Params[0].AsInteger := StrToInt(Label9.Caption);
  DM1.q4.open;

  if DM1.q4.IsEmpty then
  begin
    cidade := '';
    sigla := '';
  end
  else
  begin
    if Label9.Caption <> '1' then
    begin
      DM1.q3.Close;
      DM1.q3.SQL.Text := 'select m.nome, u.sigla from municipio m, uf u where m.ufid = u.id and m.id = :d0';
      DM1.q3.Params[0].AsInteger := DM1.q4.FieldByName('municipioid').AsInteger;
      DM1.q3.open;

      cidade := DM1.q3.FieldByName('nome').AsString;
      sigla := DM1.q3.FieldByName('sigla').AsString;
    end;
  end;

  if fimp = nil then
    fimp := Tfimp.Create(Application);

  fimp.RLLabel2.Caption := 'Nº. :' + StaticText2.Caption;
  fimp.RLLabel3.Caption := 'CLIENTE: ' + Label9.Caption + '  ' + lblConsumidor.Caption;
  // FIMP.RLLabel17.Caption:=FormatFloat('0.00', rvalor);
  // FIMP.RLLabel18.Caption:=FormatFloat('0.00', rTROCA);
  fimp.RLLabel19.Caption := FormatFloat('0.00', rSUBT);
  fimp.RLLabel20.Caption := FormatFloat('0.00', rDESCP);
  fimp.RLLabel21.Caption := FormatFloat('0.00', rAPAGAR);
  fimp.RLLabel8.Caption := 'ENDEREÇO: ' + ende + ' ,' + num + '   ' + bairro + '   ' + cidade + '  ' + sigla;
  fimp.RLLabel27.Caption := 'TELEFONE: ' + tel;

  chama_temp;

  qryConsulta.Close;
  qryConsulta.SQL.Clear;
  qryConsulta.SQL.add('select numero, ordem, vencimento, valor, ESPECIE from receber where numero = :d0');
  qryConsulta.Params[0].AsString := StaticText2.Caption;
  qryConsulta.open;
  if qryConsulta.IsEmpty then
    fimp.RLSubDetail1.Visible := false;

  fimp.RLReport1.PreviewModal;

  fimp.Release;
  fimp := nil;
  Fbalcao.SetFocus;
end;

procedure TFbalcao.busca_movimentacao;
begin
  dm.IBQuery1.Close;
  dm.IBQuery1.SQL.Clear;
  dm.IBQuery1.SQL.add('select max(id)as id, max(numero)as numero from movimentacao group by numero');
  dm.IBQuery1.open;
  dm.IBQuery1.Last;

  id := dm.IBQuery1.FieldByName('id').AsInteger;
  numero := dm.IBQuery1.FieldByName('numero').AsInteger;

end;

procedure TFbalcao.movimentacao;
begin
  qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.add('insert into movimentacao (id, numero, origem, datahoraemissao, datahoraefetivacao, datahoraentrega, modalidadefrete, observacaocomplementar, ativo, datainclusao, idplanoconta, idcentrocusto, movimentacaopacienteid, movimentacaoempresaid,');
  qryExecutar.SQL.add(' formapagamentoid, movimentacaoclienteid, movimentacaotecnicoid, movimentacaofornecedorid, movimentacaovendedorid, movimentacaousuarioid, movimentacaotransportadoraid, movimentacaofiscalid, movimentacaototalizacaoid, ');
  qryExecutar.SQL.add('movimentacaodavosid, movimentacaomesacontaclienteid, movimentacaotabeladeprecoid, movimentacaoautorizacaoid, movimentacaoconvenioid, movimentacaolotenfeid) values (:d0,:d1,:d2,:d3,:d4,:d5,:d6,:d7,:d8,:d9,:d10,:d11,:d12,');
  qryExecutar.SQL.add(':d13,:d14,:d15,:d16,:d17,:d18,:d19,:d20,:d21,:d22,:d23,:d24,:d25,:d26,:d27,:d28)');
  qryExecutar.Params[0].AsInteger := id;
  qryExecutar.Params[1].AsInteger := numero;
  qryExecutar.Params[2].AsString := 'NotadeVenda';
  qryExecutar.Params[3].AsDateTime := now;
  qryExecutar.Params[4].AsDateTime := now;
  qryExecutar.Params[5].AsDateTime := now;
  qryExecutar.Params[6].AsString := 'SemFrete';
  qryExecutar.Params[7].AsString := '';
  qryExecutar.Params[8].AsInteger := 1;
  qryExecutar.Params[9].AsDateTime := now;
  qryExecutar.Params[10].AsInteger := 1;
  qryExecutar.Params[11].AsInteger := 1;
  qryExecutar.Params[12].Clear;
  qryExecutar.Params[13].AsInteger := id;
  qryExecutar.Params[14].AsInteger := 1;
  /// se for a vista 1 a prazo 3
  qryExecutar.Params[15].AsInteger := id;
  qryExecutar.Params[16].Clear;
  qryExecutar.Params[17].Clear;
  qryExecutar.Params[18].Clear;
  qryExecutar.Params[19].AsInteger := id;
  qryExecutar.Params[20].Clear;
  qryExecutar.Params[21].AsInteger := id;
  qryExecutar.Params[22].AsInteger := id;
  qryExecutar.Params[23].Clear;
  qryExecutar.Params[24].Clear;
  qryExecutar.Params[25].Clear;
  qryExecutar.Params[26].Clear;
  qryExecutar.Params[27].Clear;
  qryExecutar.Params[28].Clear;
  qryExecutar.ExecSQL;
end;

procedure TFbalcao.prazo_01;
var
  index: Integer;
begin
  FFinalizado := '0';
  index := DM1.StartTransaction;
  try
    fparc := TFparc.Create(Application);
    fparc.sit := '1';
    fparc.index := index;
    fparc.Edit1.Text := FormatFloat('0.00', StrToFloatDef(Label16.Caption, 0));
    fparc.showmodal;
    DM1.TRANS2.Commit;
  except
    on e: Exception do
    begin
      DM1.TRANS2.Rollback;
      MessageDlg('Erro ao Gravar Receber' + #10#13 + 'Erro: ' + e.Message, mtError, [mbOK], 0);
    end;
  end;

  FreeAndNil(fparc);
  Fbalcao.SetFocus;
end;

procedure TFbalcao.ComboBox1Exit(Sender: TObject);
begin
  if (ComboBox1.ItemIndex = 0) or (ComboBox1.ItemIndex = 1) then
    vcto := date + 30;
end;

procedure TFbalcao.ComboBox1KeyPress(Sender: TObject; var Key: Char);
begin
  if Key = #13 then
    Edit2.SetFocus;
end;

procedure TFbalcao.receber_parc;
begin
  qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.add('insert into receber (numero,ordem,codigo,tipo,modelo,serie,subserie,origem,historico,emissao,vencimento,valor,operador,id_vendedor) values (:d0,:D1,:D2,:d3,:d4,:d5,:d6,:d7,:d8,:d9,:d10,:d11,:d12,:vendedor)');
  qryExecutar.ParamByName('d0').AsString := StaticText2.Caption;
  qryExecutar.ParamByName('d1').AsString := '01';
  qryExecutar.ParamByName('d2').AsInteger := StrToInt(Label9.Caption);
  qryExecutar.ParamByName('d3').AsString := '1';
  qryExecutar.ParamByName('d4').AsString := '';
  qryExecutar.ParamByName('d5').AsString := '';
  qryExecutar.ParamByName('d6').AsString := '';
  qryExecutar.ParamByName('d7').AsString := 'BA';
  qryExecutar.ParamByName('d8').AsString := 'Faturamento de nota fiscal';
  qryExecutar.ParamByName('d9').AsDate := StrToDate(Label11.Caption);
  if Parametro.PRAZO_PARCELA <> 0 then
    qryExecutar.ParamByName('d10').AsDate := Parametro.PRAZO_PARCELA + date
  ELSE
    qryExecutar.ParamByName('d10').AsDate := date + StrToInt(Edit17.Text);
  qryExecutar.ParamByName('d11').AsFloat := StrToFloat(Label16.Caption); // rAPAGAR;
  qryExecutar.ParamByName('d12').AsInteger := StrToInt(Label9.Caption);
  qryExecutar.ParamByName('vendedor').AsInteger := StrToIntDef(Edit3.Text, 0);
  qryExecutar.ExecSQL;

//  if Label9.Caption <> '0' then
//  begin
//    if Parametro.NP then
//    begin
//      iF MessageDlg('IMPRIMIR NOTA PROMISSÓRIA?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
//        imprimir_np;
//    end;
//  end;
end;

procedure TFbalcao.BitBtn5Click(Sender: TObject);
begin
  gravar;
  if import = '1' then
    limpa_mesa;
  limpa;
  Panel5.Visible := false;
end;

procedure TFbalcao.Edit5Exit(Sender: TObject);
begin
  rdesc := 0;
  if Parametro.Tipo = 3 then
  begin
    if StrToFloatDef(Edit5.Text, 0) < StrToFloatDef(Edit4.Text, 0) then
    begin
      ShowMessage('Valor a receber menos que a pagar.');
      Edit5.Clear;
      Edit5.SetFocus;
    end
    else
    begin
      Edit6.Text := FloatToStr(StrToFloatDef(Edit5.Text, 0) - StrToFloatDef(Edit4.Text, 0));
      Edit5.Text := FormatFloat('0.00', StrToFloatDef(Edit5.Text, 0));
      Edit6.Text := FormatFloat('0.00', StrToFloatDef(Edit6.Text, 0));
    end;
  end
  else
  begin
    if Edit5.Text = '0,00' then
    begin
      Edit6.Text := Label16.Caption;
    end
    else
    begin
      rdesc := StrToFloatDef(Edit5.Text, 0);

      vtotal := valtot - rdesc;
      Edit6.Text := FormatFloat('0.00', vtotal);
    end;
    Edit5.Text := FormatFloat('0.00', StrToFloatDef(Edit5.Text, 0));
  end;
end;

procedure TFbalcao.botaoF5;
begin
  Panel5.Visible := true;
  valtot := StrToFloatDef(Label16.Caption, 0);
  Edit4.Text := Label16.Caption;
  Edit5.Text := '0,00';
  Edit6.Text := Label16.Caption;
  Edit5.SetFocus;
end;

procedure TFbalcao.Timer2Timer(Sender: TObject);
var
  DiaDaSemana: string;
begin
  if GUsuario.Tipo = '0' then
  begin
    case DayOfWeek(date) of
      1:
        DiaDaSemana := 'Domingo';
      2:
        DiaDaSemana := 'Segunda-Feira';
      3:
        DiaDaSemana := 'Terça-Feira';
      4:
        DiaDaSemana := 'Quarta-Feira';
      5:
        DiaDaSemana := 'Quinta-Feira';
      6:
        DiaDaSemana := 'Sexta-Feira';
      7:
        DiaDaSemana := 'Sábado';
    end;

    stData.Caption := DiaDaSemana + ', ' + formatDateTime('dd/mm/yyyy', date);
    stHora.Caption := formatDateTime('hh:mm', now);
    stVersao.Caption := 'Term.: 00' + DM1.TERMINAL + '     v' + VersaoExe;
    stMaquinaIP.Caption := DM1.NomeTerminal + ' [' + GetLocalIP + ']';

    lblData.Caption := formatDateTime('ddddddd', date);
    lblHora.Caption := formatDateTime('hh:mm', now);
    lblHora.Refresh;
  end;
end;

function TFbalcao.GetLocalIP: string;
type
  TaPInAddr = array [0 .. 10] of PInAddr;
  PaPInAddr = ^TaPInAddr;
var
  phe: PHostEnt;
  pptr: PaPInAddr;
  Buffer: array [0 .. 63] of Ansichar;
  I: Integer;
  GInitData: TWSADATA;
begin
  WSAStartup($101, GInitData);
  result := '';
  GetHostName(Buffer, SizeOf(Buffer));
  phe := GetHostByName(Buffer);
  if phe = nil then
    exit;
  pptr := PaPInAddr(phe^.h_addr_list);
  I := 0;
  while pptr^[I] <> nil do
  begin
    result := StrPas(inet_ntoa(pptr^[I]^));
    result := StrPas(inet_ntoa(pptr^[I]^));
    Inc(I);
  end;
  WSACleanup;
end;

procedure TFbalcao.BitBtn6Click(Sender: TObject);
var
  Trans: TIBTransaction;
begin
  iF MessageDlg('TEM CERTEZA QUE DESEJA CANCELAR ESSE REGISTRO?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
  BEGIN
    DM1.Data.Connected := false; // true

    new := '0';

    cancela;

    Panel11.Visible := false;
    BitBtn1.Enabled := true;
    BitBtn3.Enabled := true;
    BitBtn4.Enabled := true;
    BitBtn7.Enabled := true;
    BitBtn6.Enabled := false;
    BitBtn2.Enabled := false;
    Edit2.Enabled := false;

    BitBtn1.SetFocus;
    // Edit3.Visible := false;
    Edit3.Text := dm.Usuarioid.AsString;
    StaticText5.Caption := dm.Usuarionomeusuario.Value;
    // Panel19.Visible := true;
    limite_ultrapassado := false;
    Trans := DM1.GetTransaction(Index);
    if Trans.InTransaction then
      Trans.Rollback;
  END;
end;

procedure TFbalcao.AlteraValorUnitrio1Click(Sender: TObject);
var
InputString : string;
begin
  if Parametro.BLOQUEIA_ALTERACAO_GRID_BALCAO = '0' then
    BEGIN
      AbreEdicaoItens(qryTMPItens, qryExecutar, StaticText2.Caption);
      atualiza;
    END
    else
    begin
//      if Parametro.BLOQUEIA_ALTERACAO_GRID_BALCAO <> '0' then
      PostMessage(Handle, InputBoxMessage, 0, 0);
      InputString := InputBox('Senha', 'Digite a senha', '');
      if InputString = Parametro.BLOQUEIA_ALTERACAO_GRID_BALCAO then
        BEGIN
          AbreEdicaoItens(qryTMPItens, qryExecutar, StaticText2.Caption);
          atualiza;
        END
        else
        begin
          ShowMessage('Senha Inválida !!!');
        end;
    end;
end;

procedure TFbalcao.Button1Click(Sender: TObject);
begin
  qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.add('update frente_tmpitvendas set qtd = :d0, preco = :d1 , total = :d2 where item = :d3 and tipo = 1');
  qryExecutar.ParamByName('d0').AsFloat := StrToFloatDef(Edit7.Text, 0);
  qryExecutar.ParamByName('d1').AsFloat := StrToFloatDef(Edit8.Text, 0);
  qryExecutar.ParamByName('d2').AsFloat := StrToFloatDef(Edit9.Text, 0);
  qryExecutar.ParamByName('d3').AsInteger := StrToInt(Label25.Caption);
  qryExecutar.ExecSQL;
  Panel9.Visible := false;

  qryTMPItens.Close;
  qryTMPItens.SQL.Text := 'select * from frente_tmpitvendas where cupom = :d0 and tipo = 1 order by cupom , item';
  qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
  qryTMPItens.open;

  atualiza;
end;

procedure TFbalcao.Edit8Exit(Sender: TObject);
begin
  Edit9.Text := FloatToStr(StrToFloatDef(Edit7.Text, 0) * StrToFloatDef(Edit8.Text, 0));
end;

procedure TFbalcao.Button2Click(Sender: TObject);
begin
  Panel9.Visible := false;
end;

procedure TFbalcao.MaskEdit1Exit(Sender: TObject);
begin
  Button1.SetFocus;
end;

procedure TFbalcao.ACBrBAL1LePeso(Peso: Double; Resposta: AnsiString);
begin
  Fbalcao.pesof := FloatToStr(Peso);
  ACBrBAL1.Desativar;
end;

procedure TFbalcao.actBuscarComandaExecute(Sender: TObject);
var
  InputValue: string;
  IntegerValue: Integer;
begin
  // Solicita ao usuário um número inteiro
  if InputQuery('Nº da comanda', 'Número da comanda:', InputValue) then
  begin
    // Tenta converter a string para inteiro
    if TryStrToInt(InputValue, IntegerValue) then
    begin
      CarregarVenda(IntegerValue);
    end
    else
    begin
      // A conversão falhou, exiba uma mensagem de erro
      ShowMessage('Por favor, insira uma comanda válida.');
    end;
  end;
end;

procedure TFbalcao.actModoEmissaoExecute(Sender: TObject);
begin
  if not (Parametro.SAT) and not (Parametro.NFCE) then
  begin
    FTransmitirDocumento := false;
    imgDoc.Visible := false;
  end
  else
  begin
    FTransmitirDocumento := not FTransmitirDocumento;
    imgDoc.Visible := FTransmitirDocumento
  end;
end;

procedure TFbalcao.actSalvarComandaExecute(Sender: TObject);
begin
  s_vendas_comanda;
  limpa;
end;

procedure TFbalcao.AlterarValoraMenor1Click(Sender: TObject);
begin
  Panel10.Visible := true;
  Edit10.SetFocus;
  Label35.Caption := dm.TempITEM.AsString;
  Label36.Caption := dm.TempDESCRICAO.Value;
  Edit10.Text := dm.TempQTD.AsString;
  Edit11.Text := dm.TempPRECO.AsString;
  Edit12.Text := dm.TempTOTAL.AsString;
end;

procedure TFbalcao.Button3Click(Sender: TObject);
begin
  Panel10.Visible := false;
end;

procedure TFbalcao.Button4Click(Sender: TObject);
begin
  qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.add('update frente_tmpitvendas set qtd = :d0, preco = :d1 , total = :d2 where item = :d3 and tipo = 1');
  qryExecutar.ParamByName('d0').AsFloat := StrToFloatDef(Edit7.Text, 0);
  qryExecutar.ParamByName('d1').AsFloat := StrToFloatDef(Edit8.Text, 0);
  qryExecutar.ParamByName('d2').AsFloat := StrToFloatDef(Edit9.Text, 0);
  qryExecutar.ParamByName('d3').AsInteger := StrToInt(Label35.Caption);
  qryExecutar.ExecSQL;
  Panel9.Visible := false;

  qryTMPItens.Close;
  qryTMPItens.SQL.Clear;
  qryTMPItens.SQL.add('select * from frente_tmpitvendas where cupom = :D0 and tipo = 1 order by cupom, item');
  qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
  qryTMPItens.open;

  atualiza;
end;

procedure TFbalcao.baixa_estoque;
begin
  qryTMPItens.Close;
  qryTMPItens.SQL.Clear;
  qryTMPItens.SQL.add('select * from frente_tmpitvendas where cupom = :d0 and tipo = 1 order by cupom, item');
  qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
  qryTMPItens.open;

  qryTMPItens.First;
  while not qryTMPItens.Eof do
  begin
    qryConsulta.Close;
    qryConsulta.SQL.Clear;
    qryConsulta.SQL.add('select quantidade from produtoempresa where id = :d0');
    qryConsulta.ParamByName('d0').AsInteger := qryTMPItens.FieldByName('codigo').AsInteger;
    qryConsulta.open;

    qryExecutar.Close;
    qryExecutar.SQL.Clear;
    qryExecutar.SQL.add('update produtoempresa set quantidade = :d0 where id = :d1');
    qryExecutar.ParamByName('d0').AsFloat := (qryConsulta.FieldByName('quantidade').AsFloat - qryTMPItens.FieldByName('qtd').AsFloat); //qryTMPItens.FieldByName('qtdbaixa').AsFloat);
    qryExecutar.ParamByName('d1').AsInteger := qryTMPItens.FieldByName('codigo').AsInteger;
    qryExecutar.ExecSQL;

    qryTMPItens.Next;
  end;
end;

procedure TFbalcao.BitBtn7Click(Sender: TObject);
begin
  if Freimp = nil then
    Freimp := TFreimp.Create(Application);
  Freimp.showmodal;
  Freimp.Release;
  Freimp := nil;
  Fbalcao.BitBtn1.SetFocus;
end;

procedure TFbalcao.imprimir_40;
var
  IMP: textfile;
  cidade, sigla: string[40];
  tel, ende, bairro, num: string;
begin
  qryTMPItens.Close;
  qryTMPItens.SQL.Clear;
  qryTMPItens.SQL.add('select * from FRENTE_TMPITVENDAS where cupom = :D0 and tipo = 1 order by cupom, item');
  qryTMPItens.Params[0].AsString := StaticText2.Caption;
  qryTMPItens.open;

  if Label9.Caption <> '1' then
  begin
    DM1.q2.Close;
    DM1.q2.SQL.Text := 'select t.numero, e.logradouro, (e.numero)as nume, e.bairro, e.municipioid from endereco e, telefone t where t.id = e.id and t.pessoaid = :d0';
    DM1.q2.Params[0].AsInteger := StrToInt(Label9.Caption);
    DM1.q2.open;

    tel := DM1.q2.FieldByName('numero').AsString;
    ende := DM1.q2.FieldByName('logradouro').AsString;
    num := DM1.q2.FieldByName('nume').AsString;
    bairro := DM1.q2.FieldByName('bairro').AsString;
  end;

  DM1.q4.Close;
  DM1.q4.SQL.Text := 'select municipioid from endereco where pessoaid = :d0';
  DM1.q4.Params[0].AsInteger := StrToInt(Label9.Caption);
  DM1.q4.open;

  if DM1.q4.IsEmpty then
  begin
    cidade := '';
    sigla := '';
  end
  else
  begin
    if Label9.Caption <> '1' then
    begin
      DM1.q3.Close;
      DM1.q3.SQL.Text := 'select m.nome, u.sigla from municipio m, uf u where m.ufid = u.id and m.id = :d0';
      DM1.q3.Params[0].AsInteger := DM1.q4.FieldByName('municipioid').AsInteger;
      DM1.q3.open;
      cidade := DM1.q3.FieldByName('nome').AsString;
      sigla := DM1.q3.FieldByName('sigla').AsString;
    end;
  end;

  AssignFile(IMP, DM1.porta);
  ReWrite(IMP);

  DM1.Q1.Close;
  DM1.Q1.SQL.Text := 'select nomefantasia from juridica where id = 32';
  DM1.Q1.open;

  // WriteLn ( Imp,#29 + #249 + #32 + #0 + #27 + #116 + #8 + #27 + #69 +'          ECOVILLE DISTRIBUIDORA MASTER  '+#27 + #70 ); //NEGRITO
  WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #27 + #69 + DM1.Q1.FieldByName('nomefantasia').AsString + #27 + #70); // NEGRITO

  // WriteLn ( Imp,#29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 +'                 RUA MAZOLINI         SOCORRO SP'+ #18 );
  // WriteLn ( Imp,#29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 +'                    13960000         SAO PAULO'+ #18 );

  // WriteLn ( Imp,#29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 + '' + #18);  //CONDENSADO
  WriteLn(IMP, '--------------------------------------------------');
  // WriteLn ( Imp,#29 + #249 + #32 + #0 + #27 + #116 + #8 + #27 + #69 +'No.: '+statictext2.Caption +#27 + #70 ); //NEGRITO
  // WriteLn ( Imp,#29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 +' No.: '+statictext2.Caption + #18 );

  // WriteLn ( Imp,#27 + #87 + #01 + 'PEDIDO: '+statictext2.Caption + #27 + #87 + #48); // EXPANDIDO

  // WriteLn ( Imp,'--------------------------------------------------' );
  // WriteLn ( Imp,'Vend.: '+statictext5.Caption);

  WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 + Label11.Caption + '                  ' + Label12.Caption + '                  ' + 'No.: ' + StaticText2.Caption + #18);

  // WriteLn ( Imp,label11.caption+'   '+label12.caption+' '+'No.: '+statictext2.Caption);

  // WriteLn ( Imp,#29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 +'Tipo: '+copy(ComboBox1.text,3,5)+ #18 );

  WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 + 'Tipo: ' + Copy(ComboBox1.Text, 3, 5) + #18);

  if Label9.Caption = '1' then
  begin
    WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #27 + #69 + 'Cliente: ' + Copy(lblConsumidor.Caption, 1, 22) + #27 + #70);
  end
  else
  begin

    try
      WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #27 + #69 + 'Cliente: ' + Copy(lblConsumidor.Caption, 1, 22) + '  ' + tel + #27 + #70);
    except
    end;

    try
      WriteLn(IMP, 'End.: ' + ende + ', No.: ' + num);
    except
    end;

    try
      WriteLn(IMP, 'Bairro: ' + bairro + '     Cidade: ' + cidade + '   ' + sigla);
    except
    end;

  end;

  WriteLn(IMP, '');
  WriteLn(IMP, '--------------------------------------------------');
  WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 + 'CODIGO        DESCRICAO PRODUTO                                UN' + #18);
  WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 + 'MARCA             QUANT.           VL. UNIT             VL. TOTAL' + #18);
  WriteLn(IMP, '--------------------------------------------------');

  chama_temp;
  qryTMPItens.First;
  while not qryTMPItens.Eof do
  begin
    WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 + AjustaStr_zero_esq(qryTMPItens.FieldByName('codigo').AsString, 5) + '         ' + AjustaStr_esq(Copy(qryTMPItens.FieldByName('descricao').AsString, 1, 40), 40) + '         ' + qryTMPItens.FieldByName('und').AsString + ' ' + preencheespaco(qryTMPItens.FieldByName('serial').AsString, 15) + '    ' +
      AjustaStr_zero_esq(qryTMPItens.FieldByName('qtd').AsString, 4) + '              ' + transform(qryTMPItens.FieldByName('preco').AsFloat) + '                ' + transform(qryTMPItens.FieldByName('total').AsFloat) + #18);

    qryTMPItens.Next;
  end;
  // #27 + #52 + 'Modo Italico' + #27 + #53 + #10
  WriteLn(IMP, '--------------------------------------------------');
  WriteLn(IMP, #27 + #52 + 'SUBTOTAL R$:               ' + transform(rSUBT) + #27 + #53);
  WriteLn(IMP, '--------------------------------------------------');
  WriteLn(IMP, #27 + #52 + 'DESCONTO R$:               ' + transform(rDESCO) + #27 + #53);
  WriteLn(IMP, '--------------------------------------------------');
  WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #27 + #69 + 'TOTAL R$:                  ' + transform(rAPAGAR) + #27 + #70);
  WriteLn(IMP, '--------------------------------------------------');

  if (ComboBox1.ItemIndex = 1) or (ComboBox1.ItemIndex = 2) then
  begin
    WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #27 + #69 + '              FORMA DE PAGAMENTO' + #27 + #70);
    WriteLn(IMP, '--------------------------------------------------');
    WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #27 + #69 + 'VENCIMENTO          ORDEM           VALOR R$     ' + #27 + #70);
    WriteLn(IMP, '--------------------------------------------------');

    qryConsulta.Close;
    qryConsulta.SQL.Clear;
    qryConsulta.SQL.add('select * from receber where numero = :d0 Order BY VENCIMENTO');
    qryConsulta.Params[0].AsString := StaticText2.Caption;
    qryConsulta.open;
    qryConsulta.First;

    while not qryConsulta.Eof do
    begin
      WriteLn(IMP, DateToStr(qryConsulta.FieldByName('VENCIMENTO').AsDateTime) + '             ' + qryConsulta.FieldByName('ORDEM').AsString + '              ' + transform(qryConsulta.FieldByName('VALOR').AsFloat));

      qryConsulta.Next;
    end;
    WriteLn(IMP, '--------------------------------------------------');
  end;

  WriteLn(IMP, '');
  WriteLn(IMP, '');
  WriteLn(IMP, '                             _____________________');
  WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 + '                                             Assinatura Cliente' + #18);
  WriteLn(IMP, '--------------------------------------------------');
  WriteLn(IMP, #29 + #249 + #32 + #0 + #27 + #116 + #8 + #15 + '                  AGRADECEMOS PELA PREFERENCIA    ' + #18);
  WriteLn(IMP, '--------------------------------------------------');
  // WriteLn ( Imp,'--------------------------------------------------' );

  WriteLn(IMP, chr(27) + chr(109));

  CloseFile(IMP);
end;

procedure TFbalcao.chama_temp;
begin
  if not Parametro.VEICULO then
  begin
    qryTMPItens.Close;
    qryTMPItens.SQL.Clear;
    qryTMPItens.SQL.add('select * from frente_tmpitvendas where cupom = :d0 and tipo = 1 order by cupom, item');
    qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
    qryTMPItens.open;

    if Parametro.laser = 3 then
    begin
      fimp.RLBand14.Visible := true;

      fimp.RLBand46.Visible := false;
    end;
  end
  else
  begin
    qryTMPItens.Close;
    qryTMPItens.SQL.Clear;
    qryTMPItens.SQL.add(' select f.cupom, f.operador, f.item, f.codigo, f.barras, f.descricao, f.qtd, f.preco, f.tributacao, f.icms, f.iss, f.und, f.serial, f.desconto, f.acrescimo, f.total, f.cancelado, f.operador_sup, f.lote, f.tabela_preco, f.grupo, ');
    qryTMPItens.SQL.add(' f.custo, f.codcar, f.modelo, (c.modelo)as modelo1, c.cil, f.modcar, f.codpro_veic from carros c, frente_tmpitvendas f, CARRO_BARRAS CA where c.id = CA.CODCAR and f.CODPRO_VEIC = CA.id and f.cupom = :d0 and f.tipo = 1 order by f.cupom, f.item');
    qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
    qryTMPItens.open;
    if Parametro.laser = 3 then
    begin
      fimp.RLBand14.Visible := false;

      fimp.RLBand46.Visible := true;
    end;
  end;
end;

procedure TFbalcao.BitBtn8Click(Sender: TObject);
begin
  dm.usuario.Close;
  dm.usuario.SQL.Clear;
  dm.usuario.SQL.add('select * from usuario where id = :d0 and senha = :d1');
  dm.usuario.ParamByName('d0').AsInteger := StrToInt(Edit14.Text);
  dm.usuario.ParamByName('d1').AsString := Edit15.Text;
  dm.usuario.open;
  if dm.usuario.IsEmpty then
  begin
    ShowMessage('Código ou Senha de usuários inexistentes.');
    Edit14.Clear;
    Edit15.Clear;
    Edit14.SetFocus;
  end
  else
  begin
    Panel11.Visible := false;
    BitBtn1.Enabled := true;
    BitBtn3.Enabled := true;
    BitBtn4.Enabled := true;
    BitBtn7.Enabled := true;

    BitBtn1.SetFocus;
    Edit3.Visible := false;
    Edit3.Text := dm.Usuarioid.AsString;
    StaticText5.Caption := dm.Usuarionomeusuario.Value;
  end;
end;

procedure TFbalcao.BitBtn9Click(Sender: TObject);
begin
  Panel12.Visible := false;

  salva_temp;

  atualiza;

  Edit1.Clear;
  Edit2.Clear;
  Edit1.SetFocus;
end;

procedure TFbalcao.BitBtn10Click(Sender: TObject);
begin
  Panel13.Visible := false;
  Edit1.SetFocus;
end;

procedure TFbalcao.limpa_mesa;
begin
  dm.Query1.Close;
  dm.Query1.SQL.Clear;
  dm.Query1.SQL.add('update frente_mesas set situacao = :d0, total = :D1, data_abertura = :d2, hora_abertura = :D3 where mesa = :d4');
  dm.Query1.Params[0].AsString := 'N';
  dm.Query1.Params[1].AsFloat := 0;
  dm.Query1.Params[2].Clear;
  dm.Query1.Params[3].Clear;
  dm.Query1.Params[4].AsInteger := mesa;
  dm.Query1.ExecSQL;

  dm.Query2.Close;
  dm.Query2.SQL.Clear;
  dm.Query2.SQL.add('delete from FRENTE_TMPITMESA where mesa = :d0');
  dm.Query2.Params[0].AsInteger := mesa;
  dm.Query2.ExecSQL;
  dm.Transaction1.Commit;
  import := '0';
end;

procedure TFbalcao.listMetodoPagtoKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
begin
  if Key = VK_RETURN then
    SelecionaMetodoPagto;

  if Key = VK_ESCAPE then
  begin
    iF MessageDlg('TEM CERTEZA QUE DESEJA CANCELAR ESSE REGISTRO?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
    BEGIN
      ShowHidePanel(pnlFormaPgto, false);
      if Edit2.CanFocus then
        Edit2.SetFocus;
    END;
  end;
end;

procedure TFbalcao.Edit2Enter(Sender: TObject);
begin
  if Panel5.Visible = true then
    Edit5.SetFocus;
end;

procedure TFbalcao.finaliza_mesa;
begin
  gravar;
  if import = '1' then
    limpa_mesa;
  limpa;
end;

procedure TFbalcao.Button5Click(Sender: TObject);
begin
  if fadmin = nil then
    fadmin := tfadmin.Create(Application);
  fadmin.showmodal;
  fadmin.Release;
  fadmin := nil;
  Fbalcao.SetFocus;
end;

procedure TFbalcao.muda_preco;
begin
  if preco = '0' then
  begin
    Label47.Caption := 'F8 - MUDAR PARA VAREJO.';
    preco := '1';
  end
  else
  begin
    Label47.Caption := 'F8 - MUDAR PARA ATACADO.';
    preco := '0';
  end;
end;

procedure TFbalcao.Edit11Exit(Sender: TObject);
begin
  Edit12.Text := FloatToStr(StrToFloatDef(Edit10.Text, 0) * StrToFloatDef(Edit11.Text, 0));
end;

procedure TFbalcao.BitBtn11Click(Sender: TObject);
begin
  if not GUsuario.Visualizar then
  begin
    if not GUsuario.Bloquear then
    begin
      frel := Tfrel.create(Application);
      try
        frel.showmodal;
      finally
        FreeAndNil(frel);
      end;
    end
    else
    begin
      ShowMessage('Você não tem autorização de acesso a esse módulo!');
    end;
  end
  else
  begin
    ShowMessage('Você não tem autorização para acessar esse módulo!');
  end;
end;

procedure TFbalcao.RadioButton2Click(Sender: TObject);
begin
  RadioButton1.Checked := false;
  RadioButton2.Checked := true;
end;

procedure TFbalcao.devolucao;
begin
  if fdev = nil then
    fdev := TFdev.Create(Application);
  fdev.showmodal;
  fdev.Release;
  fdev := nil;
  Fbalcao.BitBtn1.SetFocus;
end;

procedure TFbalcao.cancelado;
begin
  if fcancel = nil then
    fcancel := tfcancel.Create(Application);
  fcancel.showmodal;
  fcancel.Release;
  fcancel := nil;
  Fbalcao.SetFocus;
end;

procedure TFbalcao.CarregarVenda(comanda: integer);
begin

  // Select na vedas onde a comanda = parametro comanda, e lancado='ABERTO'
  Vendas.Close;
  Vendas.ParamByName('n_comanda').AsInteger := comanda;
  Vendas.open;





  StaticText2.Caption := VendasNota.AsString;
  edtComanda.Text := VendasN_COMANDA.AsString;
  edtMesa.Clear;
  ComboBox1.ItemIndex := -1;
  Edit1.Clear;
  Edit2.Clear;
  Label9.Caption := '';
  lblConsumidor.Caption := '';
  Label11.Caption := '';
  Label12.Caption := '';
  Label14.Caption := '0,0000';
  Label16.Caption := '0,00';
  Label51.Caption := '0,00';
  Edit7.Clear;
  Edit8.Clear;
  Edit9.Clear;
  Edit10.Clear;
  Edit11.Clear;
  Edit12.Clear;
  Edit13.Clear;
  Edit14.Clear;
  Edit15.Clear;
  RecebimentoParam.Clear;
  listMetodoPagto.ItemIndex := -1;
  FCPF_CNPJ := '';
  FNOME_RAZAO := '';

  // Query dos itens pelo numero da venda da query anterior

    qryTMPItens.Close;
    qryTMPItens.SQL.Text := 'select * from frente_tmpitvendas where cupom = :d0 and tipo = 1 order by cupom, item';
    qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
    qryTMPItens.open;
    TNumericField(qryTMPItens.FieldByName('preco')).DisplayFormat := ',0.00;-,0.00';
    TNumericField(qryTMPItens.FieldByName('total')).DisplayFormat := ',0.00;-,0.00';


end;

procedure TFbalcao.cdsPagtoCalcFields(DataSet: TDataSet);
begin
   dm1.qryConsulta.SQL.Text :=
   'select coalesce(tipo,''T'') as tipo, descricao ' +
   'from forma_pagamento where id=0' + dm1.cdsPagtoCODIGO_FORMA_PAGAMENTO.AsString;
    dm1.qryConsulta.open;
    dm1.cdsPagtoTIPO.AsString      := dm1.qryConsulta.FieldByName('tipo').AsString;
    dm1.cdsPagtoDescricao.AsString := dm1.qryConsulta.FieldByName('descricao').AsString;
    dm1.qryConsulta.Close;
end;

procedure TFbalcao.busca_emitente;
begin
  dm.IBQuery1.Close;
  dm.IBQuery1.SQL.Clear;
  dm.IBQuery1.SQL.add('select id from pessoa where tipocadastro = 32');
  dm.IBQuery1.open;
  cod_emi := dm.IBQuery1.FieldByName('id').AsInteger;
end;

procedure TFbalcao.mov_cliente;
begin
  dm.IBQuery2.Close;
  dm.IBQuery2.SQL.Clear;
  dm.IBQuery2.SQL.add('insert into movimentacaocliente (id , pessoaid) values (:d0, :d1)');
  dm.IBQuery2.Params[0].AsInteger := id;
  dm.IBQuery2.Params[1].AsInteger := StrToInt(Label9.Caption);
  dm.IBQuery2.ExecSQL;
end;

procedure TFbalcao.mov_usuario;
begin
  dm.IBQuery2.Close;
  dm.IBQuery2.SQL.Clear;
  dm.IBQuery2.SQL.add('insert into movimentacaousuario (id , pessoaid) values (:d0, :d1)');
  dm.IBQuery2.Params[0].AsInteger := id;
  dm.IBQuery2.Params[1].AsInteger := StrToInt(Edit14.Text);
  dm.IBQuery2.ExecSQL;
end;

procedure TFbalcao.mov_empresa;
begin
  busca_emitente;

  dm.IBQuery2.Close;
  dm.IBQuery2.SQL.Clear;
  dm.IBQuery2.SQL.add('insert into movimentacaoempresa (id , pessoaid) values (:d0,:d1)');
  dm.IBQuery2.Params[0].AsInteger := id;
  dm.IBQuery2.Params[1].AsInteger := cod_emi;
  dm.IBQuery2.ExecSQL;
end;

procedure TFbalcao.mov_totalizacao;
begin
  dm.IBQuery2.Close;
  dm.IBQuery2.SQL.Clear;
  dm.IBQuery2.SQL.add('insert into movimentacaototalizacao (id, baseicms, totalicms, baseicmssubstituicao, totalsubstituicao, totalprodutos, totalfrete, totalseguros, totaloutrasdespesas, totaldesconto, totalacrescimo, totalipi, total, totalpis, totalcofins,');
  dm.IBQuery2.SQL.add(' totalpissubstituicao, totalcofinssubstituicao, totalissqn, totalirrf, baseissqn, totalservico, totalimpostodesonerado, totalpisprodutos, totalpisservicos, totalcofinsprodutos, totalcofinsservicos, totalbruto,');
  dm.IBQuery2.SQL.add(' totalimpostoaproximado, codigomotivoimpostodesonerado, totalcreditosimples, percentualcreditosimples, calculadopelosistema) values (:d0,:d1,:d2,:d3,:d4,:d5,:d6,:d7,:d8,:d9,:d10,:d11,:d12,:d13,:d14,:d15,:d16,:d17,');
  dm.IBQuery2.SQL.add(' :d18,:d19,:d20,:d21,:d22,:d23,:d24,:d25,:d26,:d27,:d28,:d29,:d30,:d31,:d32)');
  dm.IBQuery2.Params[0].AsInteger := id;
  dm.IBQuery2.Params[1].AsFloat := 0;
  dm.IBQuery2.Params[2].AsFloat := 0;
  dm.IBQuery2.Params[3].AsFloat := 0;
  dm.IBQuery2.Params[4].AsFloat := 0;
  dm.IBQuery2.Params[5].AsFloat := 0;
  dm.IBQuery2.Params[6].AsFloat := 0;
  dm.IBQuery2.Params[7].AsFloat := 0;
  dm.IBQuery2.Params[8].AsFloat := 0;
  dm.IBQuery2.Params[9].AsFloat := 0;
  dm.IBQuery2.Params[10].AsFloat := 0;
  dm.IBQuery2.Params[11].AsFloat := 0;
  dm.IBQuery2.Params[12].AsFloat := 0;
  dm.IBQuery2.Params[13].AsFloat := 0;
  dm.IBQuery2.Params[14].AsFloat := 0;
  dm.IBQuery2.Params[15].AsFloat := 0;
  dm.IBQuery2.Params[16].AsFloat := 0;
  dm.IBQuery2.Params[17].AsFloat := 0;
  dm.IBQuery2.Params[18].AsFloat := 0;
  dm.IBQuery2.Params[19].AsFloat := 0;
  dm.IBQuery2.Params[20].AsFloat := 0;
  dm.IBQuery2.Params[21].AsFloat := 0;
  dm.IBQuery2.Params[22].AsFloat := 0;
  dm.IBQuery2.Params[23].AsFloat := 0;
  dm.IBQuery2.Params[24].AsFloat := 0;
  dm.IBQuery2.Params[25].AsFloat := 0;
  dm.IBQuery2.Params[26].AsFloat := 0;
  dm.IBQuery2.Params[27].AsFloat := 0;
  dm.IBQuery2.Params[28].AsFloat := 0;
  dm.IBQuery2.Params[29].AsFloat := 0;
  dm.IBQuery2.Params[30].AsFloat := 0;
  dm.IBQuery2.Params[31].AsFloat := 0;
  dm.IBQuery2.Params[32].AsFloat := 0;

  dm.IBQuery2.ExecSQL;

end;

procedure TFbalcao.DBGrid1DrawColumnCell(Sender: TObject; const Rect: TRect; DataCol: Integer; Column: TColumn; State: TGridDrawState);
begin
  if not odd(qryTMPItens.RecNo) then
    if not(gdSelected in State) then
    begin
      DBGrid1.Canvas.Brush.Color := clMoneyGreen;
      DBGrid1.Canvas.FillRect(Rect);
      DBGrid1.DefaultDrawDataCell(Rect, Column.Field, State);
    end;
end;

procedure TFbalcao.DBGrid1Exit(Sender: TObject);
begin
if edit2.CanFocus then edit2.SetFocus;
end;

procedure TFbalcao.FormShow(Sender: TObject);
begin
  DM1.qryConfig.Close;
  DM1.qryConfig.Params[0].AsInteger := GEmitente.IDEmitente;
  DM1.qryConfig.open;

  if (dm1.MODELO_PDV='65') then
    TipoDocAtual := tdNFCe
  else if (dm1.MODELO_PDV='59') then
    TipoDocAtual := tdCFe
  else
  if (DM1.qryConfigTIPO_APLICATIVO.Value = 'N') then
    TipoDocAtual := tdNFCe
  else if (DM1.qryConfigTIPO_APLICATIVO.Value = 'S') then
    TipoDocAtual := tdCFe
  else
  begin
    TipoDocAtual := tdOutro;
    ShowMessage('Tipo de documeto não definido.'+#13#10+'Favor, informar ao suporte técnico');
  end;

  Label30.Caption := GetDefaultPrinterName;

  BitBtn1.Enabled := true;
  BitBtn2.Enabled := false;
  BitBtn3.Enabled := true;
  BitBtn4.Enabled := true;
  BitBtn6.Enabled := false;
  BitBtn7.Enabled := true;
  new := '0';
  limpa;
  BitBtn1.SetFocus;
  tipo_balcao := '0';
  // Label31.Caption := GEmitente.Fantasia;
  pesavel_kg := 0;

  if not GUsuario.Visualizar then
  begin
    BitBtn7.Enabled := true;
    BitBtn11.Enabled := true;
  end
  else
  begin
    BitBtn7.Enabled := false;
    BitBtn11.Enabled := false;
  end;

  corbalcao;

  if not Parametro.LOGO_BALCAO then
  begin
    Image2.Visible := false;
  end
  else
  begin
    VerificarArquivo('C:\DIGISAT\SUITEG5\PEDIDOS\LOGO\LOGO.JPG');
    if LOGO_BALCAO_STATUS then
      begin
        Image2.Visible := true;
        Image2.Picture.LoadFromFile('C:\DIGISAT\SUITEG5\PEDIDOS\LOGO\LOGO.JPG');
      end
      else
      begin
        Image2.Visible := false;
      end;
  end;

  pnlCaixaFechado.Align := alClient; // Painel de caixa fechado
  lblData.Caption := formatDateTime('ddddddd', date);
  lblHora.Caption := formatDateTime('hh:mm', now);
//  FTransmitirDocumento := Parametro.SAT;
//  imgDoc.Visible := FTransmitirDocumento;

  if dm1.MODELO_PDV <> '00' then
    begin
      FTransmitirDocumento := Parametro.SAT;
      imgDoc.Visible := FTransmitirDocumento;

      if TipoDocAtual=tdCFe then
        ConfiguraSAT;

      if TipoDocAtual=tdNFCe then
        ConfiguraNFCe;

      if (Parametro.SAT) OR (Parametro.NFCE) then
        imgDoc.Visible := true
        else
        imgDoc.Visible := False;
    end
    else
    begin
      FTransmitirDocumento := false;
      imgDoc.Visible := False;
    end;
end;

procedure TFbalcao.Button6Click(Sender: TObject);
begin
  salva_temp;
  Panel16.Visible := false;
  atualiza;
  Edit2.Clear;
  Label47.Caption := '';
  Edit1.Text := '0001';
end;

procedure TFbalcao.imprimir_40_new;
var
  IMP: textfile;
  cidade, sigla: string[40];
  tel, ende, bairro, num: string;
begin
  qryTMPItens.Close;
  qryTMPItens.SQL.Clear;
  qryTMPItens.SQL.add('select * from FRENTE_TMPITVENDAS where cupom = :D0 and tipo = 1 order by cupom, item');
  qryTMPItens.Params[0].AsString := StaticText2.Caption;
  qryTMPItens.open;

  qryRecebimento.Close;
  qryRecebimento.SQL.Clear;
  qryRecebimento.SQL.Add(' select r.id , r.id_forma_pagamento, r.valor , f.descricao from recebimento_vendas r, forma_pagamento f where f.id = r.id_forma_pagamento and r.nota = :nota ');
  qryRecebimento.ParamByName('nota').Asstring := StaticText2.Caption;
  qryRecebimento.Open;

  if Label9.Caption <> '1' then
  begin
    DM1.q2.Close;
    DM1.q2.SQL.Text := 'select t.numero, e.logradouro, (e.numero)as nume, e.bairro, e.municipioid from endereco e, telefone t where t.PESSOAid = e.PESSOAid and t.pessoaid = :d0';
    DM1.q2.Params[0].AsInteger := StrToInt(Label9.Caption);
    DM1.q2.open;

    tel := DM1.q2.FieldByName('numero').AsString;
    ende := DM1.q2.FieldByName('logradouro').AsString;
    num := DM1.q2.FieldByName('nume').AsString;
    bairro := DM1.q2.FieldByName('bairro').AsString;
  end;

  DM1.q4.Close;
  DM1.q4.SQL.Text := 'select municipioid from endereco where pessoaid = :d0';
  DM1.q4.Params[0].AsInteger := StrToInt(Label9.Caption);
  DM1.q4.open;

  if DM1.q4.IsEmpty then
  begin
    cidade := '';
    sigla := '';
  end
  else
  begin
    if Label9.Caption <> '1' then
    begin
      DM1.q3.Close;
      DM1.q3.SQL.Text := 'select m.nome, u.sigla from municipio m, uf u where m.ufid = u.id and m.id = :d0';
      DM1.q3.Params[0].AsInteger := DM1.q4.FieldByName('municipioid').AsInteger;
      DM1.q3.open;

      cidade := DM1.q3.FieldByName('nome').AsString;
      sigla := DM1.q3.FieldByName('sigla').AsString;
    end;
  end;

  if fimp = nil then
    fimp := Tfimp.Create(Application);

  fimp.RLReport3.DataSource := DSTmpItens;
  fimp.RLReport2.DataSource := DSTmpItens;
  fimp.RLDBText22.DataSource := DSTmpItens;
  fimp.RLDBText23.DataSource := DSTmpItens;
  fimp.RLDBText24.DataSource := DSTmpItens;
  fimp.RLDBText25.DataSource := DSTmpItens;
  fimp.RLDBText26.DataSource := DSTmpItens;
  fimp.RLDBText27.DataSource := DSTmpItens;
  fimp.RLDBText28.DataSource := DSTmpItens;
  fimp.RLDBText63.DataSource := DSTmpItens;

  fimp.RLDBText63.DataSource := DSTmpItens;
  fimp.RLDBText64.DataSource := DSTmpItens;
  fimp.RLDBText65.DataSource := DSTmpItens;
  fimp.RLDBText66.DataSource := DSTmpItens;
  fimp.RLDBText67.DataSource := DSTmpItens;
  fimp.RLDBText68.DataSource := DSTmpItens;
  fimp.RLDBText69.DataSource := DSTmpItens;
  fimp.RLDBText70.DataSource := DSTmpItens;
  fimp.RLDBText71.DataSource := DSTmpItens;
  fimp.RLDBText72.DataSource := DSTmpItens;
  fimp.RLDBText73.DataSource := DSTmpItens;
  fimp.RLDBText75.DataSource := DSTmpItens;

  Fimp.RLSubDetail6.DataSource := DSqryConsulta01;
  Fimp.RLDBText80.DataSource := DSqryConsulta01;
  Fimp.RLDBText81.DataSource := DSqryConsulta01;
  Fimp.RLDBText82.DataSource := DSqryConsulta01;

  fimp.RLDBText29.DataSource := DSqryConsulta;
  fimp.RLDBText30.DataSource := DSqryConsulta;
  fimp.RLDBText31.DataSource := DSqryConsulta;
  fimp.RLSubDetail3.DataSource := DSqryConsulta;

  RLPrinter.PrinterName := Parametro.impressora;
  fimp.RLLabel322.Visible := true;
  fimp.RLLabel323.Visible := true;

  DM1.Q1.Close;
  DM1.Q1.SQL.Text := 'select nomefantasia from juridica where id = 32';
  DM1.Q1.open;

  // fimp.RLLabel83.Caption:=dm1.q1.FieldByName('nomefantasia').AsString;
  fimp.RLLabel83.Caption := GEmitente.Fantasia; // 'SUPERMERCADO IRMÃOS DANTAS';

  fimp.RLLabel84.Caption := Label11.Caption + '                ' + Label12.Caption + '                ' + 'No.: ' + StaticText2.Caption;

  if PARAMETRO.VENDEDOR then
  fimp.RLLabel85.Caption := 'Tipo: ' + Copy(ComboBox1.Text, 3, 5) + '  ESPECIE : ' + qryrecebimento.FieldByName('descricao').AsString + '  VENDEDOR : ' + StaticText5.Caption ELSE  //Especie;
  fimp.RLLabel85.Caption := 'Tipo: ' + Copy(ComboBox1.Text, 3, 5) + '           ESPECIE : ' + qryrecebimento.FieldByName('descricao').AsString;
  if Label9.Caption = '1' then
  begin
    fimp.RLLabel85.Caption := 'Cliente: ' + Copy(lblConsumidor.Caption, 1, 22);
  end
  else
  begin
    fimp.RLLabel86.Caption := 'Cliente: ' + Copy(lblConsumidor.Caption, 1, 22);
    fimp.RLLabel87.Caption := 'End.: ' + ende + ', No.: ' + num;
    fimp.RLLabel88.Caption := 'Bairro: ' + bairro + '     Cidade: ' + cidade + '   ' + sigla;
  end;

  if not Parametro.VEICULO then
  begin
    fimp.RLBand14.Visible := true;

    fimp.RLBand46.Visible := false;
  end
  else
  begin
    fimp.RLBand14.Visible := false;

    fimp.RLBand46.Visible := true;
  end;

  chama_temp;

  fimp.RLLabel73.Visible := false;

  fimp.RLLabel185.Visible := true;

  { fimp.RLLabel185.Caption := 'QUANTIDADE:      ' + Label14.Caption;
    fimp.RLLabel93.Caption := 'SUBTOTAL R$:     ' + transform(rSUBT + rDESCO);
    fimp.RLLabel94.Caption := 'DESCONTO R$:     ' + transform(rDESCO);
    if ComboBox1.ItemIndex = 2 then
    fimp.RLLabel95.Caption := 'TOTAL R$:            ' + Label16.Caption
    else
    fimp.RLLabel95.Caption := 'TOTAL R$:            ' + transform(rAPAGAR);
    fimp.RLLabel186.Caption := 'TROCO R$:            ' + transform(rTROCO); }

  Fimp.RLLabel329.Caption := transform(RACRESCIMO);
  fimp.RLLabel313.Caption := Label14.Caption;
  fimp.RLLabel315.Caption := transform(rSUBT - racrescimo);
  fimp.RLLabel316.Caption := transform(rDESCO);
  if ComboBox1.ItemIndex = 2 then
    fimp.RLLabel317.Caption := Label16.Caption
  else
    fimp.RLLabel317.Caption := transform(rSUBT - rDESCO); // transform(rAPAGAR);
  fimp.RLLabel323.Caption := transform(rValorPago);
  fimp.RLLabel318.Caption := transform(rTROCO);

  qryConsulta.Close;
  qryConsulta.SQL.Clear;
  qryConsulta.SQL.Text := 'select numero, ordem, vencimento, valor, ESPECIE from receber where numero = :numero and emissao = :emissao';
  qryConsulta.ParamByName('numero').AsString := StaticText2.Caption;
  qryconsulta.ParamByName('emissao').AsDate := StrToDate(Label11.Caption);
  qryConsulta.open;
  if qryConsulta.IsEmpty then
  begin
    fimp.RLSubDetail3.Visible := false;
    fimp.RLBand19.Visible := true;
  end
  else
  begin
    fimp.RLSubDetail3.Visible := true;
    fimp.RLBand19.Visible := false;
  end;

  if Parametro.especie_pgto then
  begin
    qryConsulta01.Close;
    qryConsulta01.SQL.Clear;
    qryConsulta01.SQL.Text := 'SELECT R.id_forma_pagamento, F.DESCRICAO, R.VALOR FROM recebimento_vendas R, forma_pagamento F WHERE R.id_forma_pagamento = F.id AND R.NOTA = :NOTA  ORDER BY R.id_forma_pagamento';
    qryConsulta01.ParamByName('NOTA').AsString := StaticText2.Caption;
    qryConsulta01.Open;
    if qryConsulta01.IsEmpty then
    begin
      Fimp.RLSubDetail6.Visible := False;
    end
    else
    begin
      Fimp.RLSubDetail6.Visible := true;
    end;
  end;

  if not Parametro.IMP_CNPJ then
  begin
    fimp.RLLabel117.Font.Size := 9;
    fimp.RLLabel117.Caption := 'FONE: ' + GEmitente.Telefone;
  end
  else
  begin
    fimp.RLLabel117.Font.Size := 7;
    fimp.RLLabel117.Caption := 'CNPJ Nº: ' + GEmitente.CPFCNPJ + '   FONE: ' + GEmitente.Telefone;
  end;

  fimp.rllabel118.Caption := GEmitente.Endereco;

  fimp.RLReport3.Margins.RightMargin := StrToFloatDef(DM1.MARGEMDIREITA, 0);

  fimp.RLReport3.Margins.LeftMargin := StrToFloatDef(DM1.MARGEMESQUERDA, 0);

  if parametro.IMPRIMIR_QRCODE_PIX then
     begin
       fimp.RLPIX.Picture.LoadFromFile('C:\DIGISAT\SUITEG5\PEDIDOS\LOGO\PIX.JPG');
       Fimp.RLBand56.Visible :=True;
       fimp.RLCNPJPix.Caption := gemitente.CPFCNPJ;
     end
     else
     begin
       Fimp.RLBand56.Visible := false;
     end;

  fimp.RLBand58.Visible := False;

  if parametro.PRINTDIALOG then
  fimp.RLReport3.PrintDialog := true
  else
  fimp.RLReport3.PrintDialog := false;

  fimp.RLReport3.Print;
  //fimp.RLReport3.PreviewModal;

//  if Parametro.NP then
//    imprimir_np;

  fimp.Release;
  fimp := nil;
  Fbalcao.SetFocus;
end;

procedure TFbalcao.consulta1;
var
  ver: string[1];
begin
  ver := '0';
  if Edit2.Text = '' then
  begin
    if fim = '2' then
      BitBtn3.Click; // Edit2.Text;
  end
  else
  begin
    if (Length(Edit2.Text) > 12) and (Copy(Edit2.Text, 1, 2) = '20') then
    begin
      bal := '1';
      cod := Copy(Edit2.Text, 3, 5);
      val1 := Copy(Edit2.Text, 8, 3);
      val2 := Copy(Edit2.Text, 11, 2);
      valor := val1 + ',' + val2;
      cod := TirarZeros(cod);

      DM1.Q7.Close;
      DM1.Q7.SQL.Clear;
      DM1.Q7.SQL.add('select p1.id, p1.codigobarra, p2.quantidade, p3.descricao, u.sigla, p4.precocusto, p4.precovenda from produto p1, PRODUTOEMPRESA p2, PRODUTOESERVICO p3, PRODUTOESERVICOEMPRESA p4, UNIDADEMEDIDA u where p1.id = p2.id');
      DM1.Q7.SQL.add(' and p1.id = p3.id and p1.id = p4.id and p3.unidademedidaid = u.id and p3.codigointerno = :d0 order by p3.descricao');
      DM1.Q7.ParamByName('d0').AsString := cod;
      DM1.Q7.open;

      if DM1.Q7.IsEmpty then
      begin
        ShowMessage('CÓDIGO INEXISTENTE!!!');
        Edit2.Clear;
        if Edit2.CanFocus then
          Edit2.SetFocus;
      end
      else
      begin
        Peso := StrToFloatDef(valor, 0); // FloatToStr(StrToFloatDef(valor));
        Edit1.Text := '0001';
        Edit1.Enabled := true;
        if Edit1.CanFocus then
          Edit1.SetFocus;
      end;
    end
    else
    begin
      if (Length(Edit2.Text) > 0) and (Edit2.Text[1] in ['0' .. '9']) then
      begin
        if (Length(Edit2.Text) >= 1) and (Length(Edit2.Text) <= 13) then
        begin
          DM1.Q7.Close;
          DM1.Q7.SQL.Text := 'select p1.id, p1.codigobarra, p2.quantidade, p3.descricao, u.sigla, p4.precocusto, p4.precovenda from produto p1, PRODUTOEMPRESA p2, PRODUTOESERVICO p3, PRODUTOESERVICOEMPRESA p4, UNIDADEMEDIDA u where ';
          DM1.Q7.SQL.Text := DM1.Q7.SQL.Text + 'p1.id = p2.id and p1.id = p3.id and p1.id = p4.id and p3.unidademedidaid = u.id and p1.codigobarra = :d0 order by p3.descricao';
          DM1.Q7.ParamByName('d0').AsString := Edit2.Text;
          DM1.Q7.open;
          if DM1.Q7.IsEmpty then
            ver := '0'
          else
            ver := '1';
        end;

        if ver = '0' then
        begin
          DM1.Q7.Close;
          DM1.Q7.SQL.Text := 'select p1.id, p1.codigobarra, p2.quantidade, p3.descricao, u.sigla, p4.precocusto, p4.precovenda from produto p1, PRODUTOEMPRESA p2, PRODUTOESERVICO p3, PRODUTOESERVICOEMPRESA p4, UNIDADEMEDIDA u where p1.id = ';
          DM1.Q7.SQL.Text := DM1.Q7.SQL.Text + 'p2.id and p1.id = p3.id and p1.id = p4.id and p3.unidademedidaid = u.id and p3.codigointerno = :d0 order by p3.descricao';
          DM1.Q7.Params[0].AsString := Edit2.Text;
          DM1.Q7.open;
          if DM1.Q7.IsEmpty then
            ver := '0'
          else
            ver := '1';
        end;

        if ver = '0' then
        begin
          DM1.Q7.SQL.Text := 'select p1.id, p1.codigobarra, p2.quantidade, p3.descricao, u.sigla, p4.precocusto, p4.precovenda from produto p1, PRODUTOEMPRESA p2, PRODUTOESERVICO p3, PRODUTOESERVICOEMPRESA p4, UNIDADEMEDIDA u where p1.id = ';
          DM1.Q7.SQL.Text := DM1.Q7.SQL.Text + 'p2.id and p1.id = p3.id and p1.id = p4.id and p3.unidademedidaid = u.id and p3.descricao like :d0 order by p3.descricao';
          DM1.Q7.ParamByName('d0').AsString := '%' + UpperCase(Edit2.Text) + '%';
          DM1.Q7.open;

          if DM1.Q7.IsEmpty then
          begin
            ShowMessage('NENHUM REGISTRO ENCONTRADO');
            Edit2.Clear;
            if Edit2.CanFocus then
              Edit2.SetFocus;
          end;
        end;
      end;
    end;

    if not DM1.Q7.IsEmpty then
    begin
      salva_temp;

      atualiza;

      Edit2.Clear;
      Label47.Caption := '';
      Edit1.Text := '0001';
    end;
  end;
  fim := '2';
  bal := '';
end;

procedure TFbalcao.erro_produto;
begin
  MessageDlg('PRODUTO NÃO CADASTRADO!', mtError, [mbOK], 0);
  Edit2.Clear;
  if Edit2.CanFocus then
    Edit2.SetFocus;
  Abort;
end;


//procedure TFbalcao.consulta2;
//var
//  ver: string[1];
//  soma, Peso, valpeso: Real;
//  xNomeCampo, xFiscal, xValorVenda, xFiltroAtivo, valorCalculado: string;
//begin
//  // Inicialização de variáveis
//  xdescricao := '';
//  xncm := '';
//  pacote := '0';
//  xFiscal := '';
//  ver := '0';
//  bal := '0';
//  pesof := '0,00';
//  valorCalculado := '';
//  Peso := 0;
//  soma := 0;
//  valpeso := 0;
//
//  cod := '';
//  val1 := '';
//  val2 := '';
//  valor := '';
//
//  Edit2.Text := Trim(Edit2.Text);
//
//  if Parametro.CODIGOBARRAS_LETRA then
//    if Copy(Edit2.Text, 1, 1) = 'A' then
//      Edit2.Text := SoNumero(Edit2.Text);
//
//  // Filtro de produtos ativos (só aplica se o parâmetro estiver falso)
//  if not Parametro.consulta_produto_inativo then
//    xFiltroAtivo := ' and p3.ativo = 1 '
//  else
//    xFiltroAtivo := '';
//
//  // Determinar campo de código
//  if Parametro.CODIGO_POR_CINTERNO then
//    xNomeCampo := IfThen(Length(Edit2.Text) < 6, 'p3.codigointerno', 'p3.id')
//  else
//    xNomeCampo := IfThen(Length(Edit2.Text) < 6, 'p3.codigointerno', 'p3.id');
//
//  // Valor de venda conforme configuração
//  if (Parametro.VALOR_VENDA = 0) or (Parametro.VALOR_VENDA = 1) then
//  begin
//    if vtaxa = 0 then
//      xValorVenda := 'p4.precovenda'
//    else
//      xValorVenda := '(((p4.precovenda * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.precovenda) as precovenda';
//  end
//  else if Parametro.VALOR_VENDA = 2 then
//  begin
//    if vtaxa = 0 then
//      xValorVenda := 'p4.atacado as precovenda'
//    else
//      xValorVenda := '(((p4.atacado * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.atacado) as precovenda';
//  end
//  else if Parametro.VALOR_VENDA = 3 then
//  begin
//    if vtaxa = 0 then
//      xValorVenda := 'p4.preco3 as precovenda'
//    else
//      xValorVenda := '(((p4.preco3 * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.preco3) as precovenda';
//  end;
//
//  // Campos fiscais
//  if Parametro.SAT or Parametro.NFCE then
//    xFiscal := ', p3.ncm, p3.csosn, p3.cfop, p3.cst, p3.cst_origem'
//  else
//    xFiscal := '';
//
//  // Consulta preliminar para verificar se é pesável
//  qryConsulta.Close;
//  qryConsulta.SQL.Text :=
//    'SELECT p1.id, p1.codigobarra, p1.pesavel, p3.descricao, p3.caracteristica, ' +
//    xValorVenda + ', p3.un_medida, p3.codigointerno' + xFiscal +
//    ' FROM produto p1 ' +
//    ' JOIN produtoeservico p3 ON p1.id = p3.id ' +
//    ' JOIN produtoeservicoempresa p4 ON p3.id = p4.id ' +
//    ' WHERE ' + xNomeCampo + ' = :codbarra ' + xFiltroAtivo;
//
//  qryConsulta.ParamByName('codbarra').AsString := Edit2.Text;
//  qryConsulta.Open;
//
//  // Se produto for pesável e unidade for KG, calcular o valor pelo peso
//  if not qryConsulta.IsEmpty and
//     (qryConsulta.FieldByName('pesavel').AsInteger = 1) and
//     (qryConsulta.FieldByName('un_medida').AsString = 'KG') then
//  begin
//    pesavel_kg := 1;
//    if Parametro.USA_BALANCA then
//      balanca
//    else
//      pesof := Edit1.Text;
//
//    Peso := StrToFloatDef(pesof, 0);
//    if Peso <= 0 then
//    begin
//      MessageDlg('Peso inválido. Favor verificar a balança.', mtError, [mbOK], 0);
//      Edit2.Clear;
//      if Edit2.CanFocus then Edit2.SetFocus;
//      Exit;
//    end;
//
//    bal := '1';
//    soma := qryConsulta.FieldByName('precovenda').AsFloat;
//    valpeso := Peso * soma;
//    valorCalculado := FormatFloat('0.00', valpeso);
//  end;
//
//  // Agora carregar os dados completos do produto (para pesável ou não)
//  qryProduto.Close;
//  qryProduto.SQL.Text :=
//    'SELECT p1.id, p1.codigobarra, p2.quantidade, p3.descricao, p3.caracteristica, p3.un_medida, ' +
//    'p4.precocusto, ' + xValorVenda + ', p4.atacado, p4.preco3' + xFiscal +
//    ' FROM produto p1 ' +
//    ' JOIN produtoempresa p2 ON p1.id = p2.id ' +
//    ' JOIN produtoeservico p3 ON p1.id = p3.id ' +
//    ' JOIN produtoeservicoempresa p4 ON p1.id = p4.id ' +
//    ' WHERE ' + xNomeCampo + ' = :codbarra ' + xFiltroAtivo +
//    ' ORDER BY p3.descricao';
//
//  qryProduto.ParamByName('codbarra').AsString := Edit2.Text;
//  qryProduto.Open;
//
//  if qryProduto.IsEmpty then
//  begin
//    erro_produto;
//    Exit;
//  end;
//
//  // Dados comuns
//  xdescricao := qryProduto.FieldByName('descricao').AsString;
//  if Parametro.SAT or Parametro.NFCE then
//    xncm := qryProduto.FieldByName('ncm').AsString
//  else
//    xncm := '00000000';
//
//  // Se for pesável, manter peso; se não, selecionar item padrão
//  if bal = '1' then
//  begin
//    valor := valorCalculado;
//    Edit1.Text := '0001';
//  end
//  else
//  begin
//    selecionar_item;
//  end;
//
//  salva_Itens_temp;
//  atualiza;
//
//  Edit2.Clear;
//  Label47.Caption := '';
//  Edit1.Text := '0001';
//  fim := '2';
//  bal := '';
//end;

//procedure TFbalcao.consulta2;
//var
//  ver: string[1];
//  soma, Peso, valpeso: Real;
//  xNomeCampo, xFiscal, xValorVenda, xFiltroAtivo, valorCalculado: string;
//  codInt: Integer;
//begin
//  // Inicialização de variáveis
//  xdescricao := '';
//  xncm := '';
//  pacote := '0';
//  xFiscal := '';
//  ver := '0';
//  bal := '0';
//  pesof := '0,00';
//  valorCalculado := '';
//  Peso := 0;
//  soma := 0;
//  valpeso := 0;
//
//  cod := '';
//  val1 := '';
//  val2 := '';
//  valor := '';
//
//  Edit2.Text := Trim(Edit2.Text);
//
//  if Parametro.CODIGOBARRAS_LETRA then
//    if Copy(Edit2.Text, 1, 1) = 'A' then
//      Edit2.Text := SoNumero(Edit2.Text);
//
//  // Filtro de produtos ativos
//  if not Parametro.consulta_produto_inativo then
//    xFiltroAtivo := ' and p3.ativo = 1 '
//  else
//    xFiltroAtivo := '';
//
//  // Verifica se é um ID (número inteiro)
//  if TryStrToInt(Edit2.Text, codInt) then
//    xNomeCampo := 'p1.id'
//  else if Parametro.CODIGO_POR_CINTERNO then
//    xNomeCampo := 'p3.codigointerno'
//  else
//    xNomeCampo := 'p1.codigobarra';
//
//  // Valor de venda conforme configuração
//  if (Parametro.VALOR_VENDA = 0) or (Parametro.VALOR_VENDA = 1) then
//  begin
//    if vtaxa = 0 then
//      xValorVenda := 'p4.precovenda'
//    else
//      xValorVenda := '(((p4.precovenda * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.precovenda) as precovenda';
//  end
//  else if Parametro.VALOR_VENDA = 2 then
//  begin
//    if vtaxa = 0 then
//      xValorVenda := 'p4.atacado as precovenda'
//    else
//      xValorVenda := '(((p4.atacado * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.atacado) as precovenda';
//  end
//  else if Parametro.VALOR_VENDA = 3 then
//  begin
//    if vtaxa = 0 then
//      xValorVenda := 'p4.preco3 as precovenda'
//    else
//      xValorVenda := '(((p4.preco3 * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.preco3) as precovenda';
//  end;
//
//  if Parametro.SAT or Parametro.NFCE then
//    xFiscal := ', p3.ncm, p3.csosn, p3.cfop, p3.cst, p3.cst_origem'
//  else
//    xFiscal := '';
//
//  // Consulta preliminar
//  qryConsulta.Close;
//  qryConsulta.SQL.Text :=
//    'SELECT p1.id, p1.codigobarra, p1.pesavel, p3.descricao, p3.caracteristica, ' +
//    xValorVenda + ', p3.un_medida, p3.codigointerno' + xFiscal +
//    ' FROM produto p1 ' +
//    ' JOIN produtoeservico p3 ON p1.id = p3.id ' +
//    ' JOIN produtoeservicoempresa p4 ON p3.id = p4.id ' +
//    ' WHERE ' + xNomeCampo + ' = :codbarra ' + xFiltroAtivo;
//
//  qryConsulta.ParamByName('codbarra').AsString := Edit2.Text;
//  qryConsulta.Open;
//
//  if not qryConsulta.IsEmpty and
//     (qryConsulta.FieldByName('pesavel').AsInteger = 1) and
//     (qryConsulta.FieldByName('un_medida').AsString = 'KG') then
//  begin
//    pesavel_kg := 1;
//    if Parametro.USA_BALANCA then
//      balanca
//    else
//      pesof := Edit1.Text;
//
//    Peso := StrToFloatDef(pesof, 0);
//    if Peso <= 0 then
//    begin
//      MessageDlg('Peso inválido. Favor verificar a balança.', mtError, [mbOK], 0);
//      Edit2.Clear;
//      if Edit2.CanFocus then Edit2.SetFocus;
//      Exit;
//    end;
//
//    bal := '1';
//    soma := qryConsulta.FieldByName('precovenda').AsFloat;
//    valpeso := Peso * soma;
//    valorCalculado := FormatFloat('0.00', valpeso);
//  end;
//
//  qryProduto.Close;
//  qryProduto.SQL.Text :=
//    'SELECT p1.id, p1.codigobarra, p2.quantidade, p3.descricao, p3.caracteristica, p3.un_medida, ' +
//    'p4.precocusto, ' + xValorVenda + ', p4.atacado, p4.preco3' + xFiscal +
//    ' FROM produto p1 ' +
//    ' JOIN produtoempresa p2 ON p1.id = p2.id ' +
//    ' JOIN produtoeservico p3 ON p1.id = p3.id ' +
//    ' JOIN produtoeservicoempresa p4 ON p1.id = p4.id ' +
//    ' WHERE ' + xNomeCampo + ' = :codbarra ' + xFiltroAtivo +
//    ' ORDER BY p3.descricao';
//
//  qryProduto.ParamByName('codbarra').AsString := Edit2.Text;
//  qryProduto.Open;
//
//  if qryProduto.IsEmpty then
//  begin
//    erro_produto;
//    Exit;
//  end;
//
//  xdescricao := qryProduto.FieldByName('descricao').AsString;
//  if Parametro.SAT or Parametro.NFCE then
//    xncm := qryProduto.FieldByName('ncm').AsString
//  else
//    xncm := '00000000';
//
//  if bal = '1' then
//  begin
//    valor := valorCalculado;
//    Edit1.Text := '0001';
//  end
//  else
//  begin
//    selecionar_item;
//  end;
//
//  salva_Itens_temp;
//  atualiza;
//
//  Edit2.Clear;
//  Label47.Caption := '';
//  Edit1.Text := '0001';
//  fim := '2';
//  bal := '';
//end;

//procedure TFbalcao.consulta2;
//var
//  ver: string[1];
//  soma, Peso, valpeso: Real;
//  xNomeCampo, xFiscal, xValorVenda, xFiltroAtivo, valorCalculado: string;
//  codInt: Integer;
//begin
//  // Inicialização de variáveis
//  xdescricao := '';
//  xncm := '';
//  pacote := '0';
//  xFiscal := '';
//  ver := '0';
//  bal := '0';
//  pesof := '0,00';
//  valorCalculado := '';
//  Peso := 0;
//  soma := 0;
//  valpeso := 0;
//
//  cod := '';
//  val1 := '';
//  val2 := '';
//  valor := '';
//
//  Edit2.Text := Trim(Edit2.Text);
//
//  if Parametro.CODIGOBARRAS_LETRA then
//    if Copy(Edit2.Text, 1, 1) = 'A' then
//      Edit2.Text := SoNumero(Edit2.Text);
//
//  // Filtro de produtos ativos
//  if not Parametro.consulta_produto_inativo then
//    xFiltroAtivo := ' and p3.ativo = 1 '
//  else
//    xFiltroAtivo := '';
//
//  // Verifica qual campo será usado na consulta
//  if Length(Edit2.Text) = 8 then
//    xNomeCampo := 'p1.codigobarra'
//  else if TryStrToInt(Edit2.Text, codInt) then
//    xNomeCampo := 'p1.id'
//  else if Parametro.CODIGO_POR_CINTERNO then
//    xNomeCampo := 'p3.codigointerno'
//  else
//    xNomeCampo := 'p1.codigobarra';
//
//  // Valor de venda conforme configuração
//  if (Parametro.VALOR_VENDA = 0) or (Parametro.VALOR_VENDA = 1) then
//  begin
//    if vtaxa = 0 then
//      xValorVenda := 'p4.precovenda'
//    else
//      xValorVenda := '(((p4.precovenda * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.precovenda) as precovenda';
//  end
//  else if Parametro.VALOR_VENDA = 2 then
//  begin
//    if vtaxa = 0 then
//      xValorVenda := 'p4.atacado as precovenda'
//    else
//      xValorVenda := '(((p4.atacado * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.atacado) as precovenda';
//  end
//  else if Parametro.VALOR_VENDA = 3 then
//  begin
//    if vtaxa = 0 then
//      xValorVenda := 'p4.preco3 as precovenda'
//    else
//      xValorVenda := '(((p4.preco3 * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.preco3) as precovenda';
//  end;
//
//  if Parametro.SAT or Parametro.NFCE then
//    xFiscal := ', p3.ncm, p3.csosn, p3.cfop, p3.cst, p3.cst_origem'
//  else
//    xFiscal := '';
//
//  // Consulta preliminar
//  qryConsulta.Close;
//  qryConsulta.SQL.Text :=
//    'SELECT p1.id, p1.codigobarra, p1.pesavel, p3.descricao, p3.caracteristica, ' +
//    xValorVenda + ', p3.un_medida, p3.codigointerno' + xFiscal +
//    ' FROM produto p1 ' +
//    ' JOIN produtoeservico p3 ON p1.id = p3.id ' +
//    ' JOIN produtoeservicoempresa p4 ON p3.id = p4.id ' +
//    ' WHERE ' + xNomeCampo + ' = :codbarra ' + xFiltroAtivo;
//
//  qryConsulta.ParamByName('codbarra').AsString := Edit2.Text;
//  qryConsulta.Open;
//
//  if not qryConsulta.IsEmpty and
//     (qryConsulta.FieldByName('pesavel').AsInteger = 1) and
//     (qryConsulta.FieldByName('un_medida').AsString = 'KG') then
//  begin
//    pesavel_kg := 1;
//    if Parametro.USA_BALANCA then
//      balanca
//    else
//      pesof := Edit1.Text;
//
//    Peso := StrToFloatDef(pesof, 0);
//    if Peso <= 0 then
//    begin
//      MessageDlg('Peso inválido. Favor verificar a balança.', mtError, [mbOK], 0);
//      Edit2.Clear;
//      if Edit2.CanFocus then Edit2.SetFocus;
//      Exit;
//    end;
//
//    bal := '1';
//    soma := qryConsulta.FieldByName('precovenda').AsFloat;
//    valpeso := Peso * soma;
//    valorCalculado := FormatFloat('0.00', valpeso);
//  end;
//
//  qryProduto.Close;
//  qryProduto.SQL.Text :=
//    'SELECT p1.id, p1.codigobarra, p2.quantidade, p3.descricao, p3.caracteristica, p3.un_medida, ' +
//    'p4.precocusto, ' + xValorVenda + ', p4.atacado, p4.preco3' + xFiscal +
//    ' FROM produto p1 ' +
//    ' JOIN produtoempresa p2 ON p1.id = p2.id ' +
//    ' JOIN produtoeservico p3 ON p1.id = p3.id ' +
//    ' JOIN produtoeservicoempresa p4 ON p1.id = p4.id ' +
//    ' WHERE ' + xNomeCampo + ' = :codbarra ' + xFiltroAtivo +
//    ' ORDER BY p3.descricao';
//
//  qryProduto.ParamByName('codbarra').AsString := Edit2.Text;
//  qryProduto.Open;
//
//  if qryProduto.IsEmpty then
//  begin
//    erro_produto;
//    Exit;
//  end;
//
//  xdescricao := qryProduto.FieldByName('descricao').AsString;
//  if Parametro.SAT or Parametro.NFCE then
//    xncm := qryProduto.FieldByName('ncm').AsString
//  else
//    xncm := '00000000';
//
//  if bal = '1' then
//  begin
//    valor := valorCalculado;
//    Edit1.Text := '0001';
//  end
//  else
//  begin
//    selecionar_item;
//  end;
//
//  salva_Itens_temp;
//  atualiza;
//
//  Edit2.Clear;
//  Label47.Caption := '';
//  Edit1.Text := '0001';
//  fim := '2';
//  bal := '';
//end;

procedure TFbalcao.consulta2;
var
  ver: string[1];
  soma, Peso, valpeso: Real;
  xNomeCampo, xFiscal, xValorVenda, xFiltroAtivo, valorCalculado: string;
  codInt: Integer;
  sCodigo: string;
  L: Integer;
  SoDigitos: Boolean;
begin
  // Inicialização de variáveis
  xdescricao := '';
  xncm := '';
  pacote := '0';
  xFiscal := '';
  ver := '0';
  bal := '0';
  pesof := '0,00';
  valorCalculado := '';
  Peso := 0;
  soma := 0;
  valpeso := 0;

  cod := '';
  val1 := '';
  val2 := '';
  valor := '';

  // Normalização do texto digitado
  Edit2.Text := Trim(Edit2.Text);
  sCodigo := Edit2.Text;

  if Parametro.CODIGOBARRAS_LETRA then
    if Copy(sCodigo, 1, 1) = 'A' then
      sCodigo := SoNumero(sCodigo); // mantém apenas dígitos

  // Reaplica o valor normalizado no Edit2
  Edit2.Text := sCodigo;

  // Calcula comprimento e checa se são apenas dígitos (sem depender de Integer)
  L := Length(sCodigo);
  SoDigitos := (sCodigo <> '') and (StrToIntDef(sCodigo, -1) <> -1);
  // Observação: StrToIntDef aqui não é para usar o número como Integer, e sim só para
  // uma checagem rápida de "todos os caracteres são dígitos". Se preferir, pode trocar por:
  // SoDigitos := sCodigo.ToCharArray.All(Char.IsDigit);

  // Filtro de produtos ativos
  if not Parametro.consulta_produto_inativo then
    xFiltroAtivo := ' and p3.ativo = 1 '
  else
    xFiltroAtivo := '';

  // >>> PONTO ALTERADO <<<
  // Regra: usar campo 'p1.codigobarra' quando o conteúdo tiver 8, 13 ou 14 dígitos.
  // Caso contrário, seguir as demais regras (id, codigointerno, etc.).
  if SoDigitos and ((L = 8) or (L = 13) or (L = 14)) then
    xNomeCampo := 'p1.codigobarra'
  else if TryStrToInt(sCodigo, codInt) then
    xNomeCampo := 'p1.id'
  else if Parametro.CODIGO_POR_CINTERNO then
    xNomeCampo := 'p3.codigointerno'
  else
    xNomeCampo := 'p1.codigobarra';

  // Valor de venda conforme configuração
  if (Parametro.VALOR_VENDA = 0) or (Parametro.VALOR_VENDA = 1) then
  begin
    if vtaxa = 0 then
      xValorVenda := 'p4.precovenda'
    else
      xValorVenda := '(((p4.precovenda * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.precovenda) as precovenda';
  end
  else if Parametro.VALOR_VENDA = 2 then
  begin
    if vtaxa = 0 then
      xValorVenda := 'p4.atacado as precovenda'
    else
      xValorVenda := '(((p4.atacado * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.atacado) as precovenda';
  end
  else if Parametro.VALOR_VENDA = 3 then
  begin
    if vtaxa = 0 then
      xValorVenda := 'p4.preco3 as precovenda'
    else
      xValorVenda := '(((p4.preco3 * ' + FloatToStr(vtaxa) + ') / 100 ) + p4.preco3) as precovenda';
  end;

  if Parametro.SAT or Parametro.NFCE then
    xFiscal := ', p3.ncm, p3.csosn, p3.cfop, p3.cst, p3.cst_origem'
  else
    xFiscal := '';

  // Consulta preliminar (para checar pesável KG e eventualmente calcular o valor pelo peso)
  qryConsulta.Close;
  qryConsulta.SQL.Text :=
    'SELECT p1.id, p1.codigobarra, p1.pesavel, p3.descricao, p3.caracteristica, ' +
    xValorVenda + ', p3.un_medida, p3.codigointerno' + xFiscal +
    ' FROM produto p1 ' +
    ' JOIN produtoeservico p3 ON p1.id = p3.id ' +
    ' JOIN produtoeservicoempresa p4 ON p3.id = p4.id ' + // p4 ligado a p3.id
    ' WHERE ' + xNomeCampo + ' = :codbarra ' + xFiltroAtivo;

  qryConsulta.ParamByName('codbarra').AsString := sCodigo;
  qryConsulta.Open;

  if not qryConsulta.IsEmpty and
     (qryConsulta.FieldByName('pesavel').AsInteger = 1) and
     (qryConsulta.FieldByName('un_medida').AsString = 'KG') then
  begin
    pesavel_kg := 1;
    if Parametro.USA_BALANCA then
      balanca
    else
      pesof := Edit1.Text;

    Peso := StrToFloatDef(pesof, 0);
    if Peso <= 0 then
    begin
      MessageDlg('Peso inválido. Favor verificar a balança.', mtError, [mbOK], 0);
      Edit2.Clear;
      if Edit2.CanFocus then Edit2.SetFocus;
      Exit;
    end;

    bal := '1';
    soma := qryConsulta.FieldByName('precovenda').AsFloat;
    valpeso := Peso * soma;
    valorCalculado := FormatFloat('0.00', valpeso);
  end;

  // Consulta principal (estoque/empresa)
  qryProduto.Close;
  qryProduto.SQL.Text :=
    'SELECT p1.id, p1.codigobarra, p2.quantidade, p3.descricao, p3.caracteristica, p3.un_medida, ' +
    'p4.precocusto, ' + xValorVenda + ', p4.atacado, p4.preco3' + xFiscal +
    ' FROM produto p1 ' +
    ' JOIN produtoempresa p2 ON p1.id = p2.id ' +
    ' JOIN produtoeservico p3 ON p1.id = p3.id ' +
    ' JOIN produtoeservicoempresa p4 ON p3.id = p4.id ' + // >>> AJUSTE AQUI: p4 atrelado a p3.id
    ' WHERE ' + xNomeCampo + ' = :codbarra ' + xFiltroAtivo +
    ' ORDER BY p3.descricao';

  qryProduto.ParamByName('codbarra').AsString := sCodigo;
  qryProduto.Open;

  if qryProduto.IsEmpty then
  begin
    erro_produto;
    Exit;
  end;

  xdescricao := qryProduto.FieldByName('descricao').AsString;

  if Parametro.SAT or Parametro.NFCE then
    xncm := qryProduto.FieldByName('ncm').AsString
  else
    xncm := '00000000';

  if bal = '1' then
  begin
    valor := valorCalculado;
    Edit1.Text := '0001';
  end
  else
  begin
    selecionar_item;
  end;

  salva_Itens_temp;
  atualiza;

  Edit2.Clear;
  Label47.Caption := '';
  Edit1.Text := '0001';
  fim := '2';
  bal := '';
end;


procedure TFbalcao.ProcAlteraValor(Sender: TComponent; AResult: Integer);
begin
  if AResult = mrOk then
  begin
    vProduto := Fbalcao.edtValor.Value;
  end;
end;

procedure TFbalcao.salva_Itens_temp;
var
  v1, v2, v3, v4, v5, Q1, V6, q2, q3: real;
  ListPrecoAtacado: TListPrecoAtacado;
  descricaoOriginal, descricaoLimpa: string;

  procedure SolicitaValorProduto;
  begin
    edtValor.Clear;
    MsgCompCall(ProcAlteraValor, self, pnlValor, 'Valor unit R$', '', nil);
  end;

begin
  vProduto := 0;

  if (Parametro.SAT) OR (Parametro.NFCE) then
  begin
    if imgDoc.Visible then
    begin
      qryFiscal.Close;
      qryFiscal.SQL.Clear;
      qryFiscal.SQL.add('select codigo from ibpt where codigo = :codigo');
      qryFiscal.ParamByName('codigo').AsString := xncm; // qryConsulta.FieldByName('ncm').AsString;
      qryFiscal.open;
      if qryFiscal.IsEmpty then
      begin
        MessageDlg('NCM ' + qryConsulta.FieldByName('ncm').AsString + ' - ' + qryConsulta.FieldByName('descricao').AsString + ' não é Válido!', mtError, [mbOK], 0);
        Edit2.Clear;
        if Edit2.CanFocus then
          Edit2.SetFocus;
        exit;
      end;

      if qryProduto.FieldByName('cfop').AsString = '' then
        begin
          MessageDlg('CFOP ' + qryConsulta.FieldByName('CFOP').AsString + ' - ' + qryConsulta.FieldByName('descricao').AsString + ' não é Válido!', mtError, [mbOK], 0);
          Edit2.Clear;
          if Edit2.CanFocus then
              Edit2.SetFocus;
          exit;
        end;

      if qryProduto.FieldByName('csosn').AsString = '' then
        begin
          MessageDlg('CSOSN ' + qryConsulta.FieldByName('CSOSN').AsString + ' - ' + qryConsulta.FieldByName('descricao').AsString + ' não é Válido!', mtError, [mbOK], 0);
          Edit2.Clear;
          if Edit2.CanFocus then
              Edit2.SetFocus;
          exit;
        end;

      if qryProduto.FieldByName('cst_origem').AsString = '' then
        begin
          MessageDlg('CST ORIGEM ' + qryConsulta.FieldByName('CST_ORIGEM').AsString + ' - ' + qryConsulta.FieldByName('descricao').AsString + ' não é Válido!', mtError, [mbOK], 0);
          Edit2.Clear;
          if Edit2.CanFocus then
              Edit2.SetFocus;
          exit;
        end;
    end;
  end;

  qryExecutar.Close;
  qryExecutar.SQL.Clear;
  qryExecutar.SQL.add(' insert into frente_tmpitvendas (cupom,N_caixa,data,hora,operador,item,codigo,barras,descricao,qtd,preco,tributacao,icms,iss,und,desconto,acrescimo,total,serial,precocusto,qtdbaixa,tipo) ');
  qryExecutar.SQL.Add(' values ');
  qryExecutar.SQL.add(' (:cupom,:n_caixa,:data,:hora,:operador,:item,:codigo,:barras,:descricao,:qtd,:preco,:tributacao,:icms,:iss,:und,:desconto,:acrescimo,:total,:serial,:precocusto,:qtdbaixa,:tipo)');
  qryExecutar.ParamByName('cupom').AsString := StaticText2.Caption;
  qryExecutar.ParamByName('n_caixa').AsString := '1';
  qryExecutar.ParamByName('data').AsDate := date;
  qryExecutar.ParamByName('hora').AsTime := time;
  qryExecutar.ParamByName('operador').AsInteger := StrToInt(Edit3.Text);
  qryExecutar.ParamByName('item').AsInteger := contador;
  qryExecutar.ParamByName('tipo').AsInteger := 1;

  qryExecutar.ParamByName('qtdbaixa').AsFloat := 0;

  if qryProduto.FieldByName('id').AsInteger = 0 then
  begin
    if FTransmitirDocumento then
    begin
      msgErro('Produto inválido', 'Não permitido produto diversos para este tipo de venda');
      Abort;
    end;

    SolicitaValorProduto;
  end;

  if not Parametro.PRODUTO then
  begin
    qryExecutar.ParamByName('codigo').AsInteger := qryProduto.FieldByName('id').AsInteger;
    qryExecutar.ParamByName('barras').AsString := qryProduto.FieldByName('codigobarra').AsString;

    descricaoOriginal := Copy(qryProduto.FieldByName('descricao').AsString, 1, 75);
    descricaoLimpa := LimparDescricao(descricaoOriginal);

    qryExecutar.ParamByName('descricao').AsString := descricaolimpa; //Copy(qryProduto.FieldByName('descricao').AsString, 1, 75);
    v1 := 0;
    v2 := 0;
    v3 := 0;
    v4 := 0;
    v5 := 0;
    V6 := 0;
    if bal = '1' then
    begin
      v1 := Peso;
    end
    else
    begin
      // v1:=dm1.qryProduto.FieldByName('precovenda').AsString;
      if not Parametro.LIB_ATACADO_BALCAO then   //alterado 20/08/22
      begin
        if selecao_qtd = 0 then
          v1 := qryProduto.FieldByName('precovenda').AsFloat
        else
          v1 := selecao_valor;
      end
      else
      begin
        if StrToFloatDef(Edit1.Text, 0) < Parametro.QUANT_PRODUTO then
        begin
          v1 := qryProduto.FieldByName('atacado').AsFloat;
          if pacote = '0' then
            if selecao_qtd = 0 then
              v1 := qryProduto.FieldByName('precovenda').AsFloat
            else
              v1 := selecao_valor;
        end
        else
          v1 := qryProduto.FieldByName('atacado').AsFloat;
      end;
    end;

    if qryProduto.FieldByName('id').AsInteger = 0 then
      v1 := vProduto;

    v2 := (v1 / 100) * 85;
    v3 := v2 + ((v2 / 100) * 60);
    v4 := StrToFloatDef(Edit1.Text, 0);
    v5 := v1 * v4;
    q2 := 0;
    q3 := 0;
    V6 := 0;

    if bal = '1' then
    begin
      q2 := Peso;

      if selecao_qtd = 0 then
        q3 := qryProduto.FieldByName('precovenda').AsFloat
      else
        q3 := selecao_valor;

      V6 := q3 / q2;
    end;

    if pacote = '0' then
    begin
      if bal = '1' then
        qryExecutar.ParamByName('qtd').AsFloat := V6
      else
        if selecao_qtd = 0 then
          qryExecutar.ParamByName('qtd').AsFloat := StrToFloatDef(Edit1.Text, 0)
        else
          qryExecutar.ParamByName('qtd').AsFloat := selecao_qtd;
    end
    else
    begin
      if bal = '1' then
        qryExecutar.ParamByName('qtd').AsFloat := V6
      else
        qryExecutar.ParamByName('qtd').AsFloat := 10; // StrToFloatDef(edit1.Text);
    end;
    // if bal = '1' then .ParamByName('d9').AsFloat:= peso else .ParamByName('d9').AsFloat:=StrToFloatDef(edit1.Text);

    if bal = '1' then
      qryExecutar.ParamByName('qtd').AsFloat := q3;

    qryExecutar.ParamByName('preco').AsFloat := v1;
    qryExecutar.ParamByName('tributacao').AsString := '';
    qryExecutar.ParamByName('icms').AsFloat := v3;
    qryExecutar.ParamByName('iss').AsFloat := 0;
    qryExecutar.ParamByName('und').AsString := qryProduto.FieldByName('sigla').AsString;
    qryExecutar.ParamByName('desconto').AsFloat := 0;
    qryExecutar.ParamByName('acrescimo').AsFloat := 0;
    if selecao_qtd = 0 then
      qryExecutar.ParamByName('total').AsFloat := v5
    else
      qryExecutar.ParamByName('total').AsFloat := selecao_valor;
    qryExecutar.ParamByName('serial').AsString := qryProduto.FieldByName('caracteristica').AsString;  //atacado;
    qryExecutar.ParamByName('precocusto').AsFloat := qryProduto.FieldByName('precocusto').AsFloat;
    qryExecutar.ExecSQL;
  end
  else
  begin
    qryExecutar.ParamByName('codigo').AsInteger := qryProduto.FieldByName('id').AsInteger;
    qryExecutar.ParamByName('barras').AsString := qryProduto.FieldByName('codigobarra').AsString;

    descricaoOriginal := Copy(qryProduto.FieldByName('descricao').AsString, 1, 75);
    descricaoLimpa := LimparDescricao(descricaoOriginal);

    qryExecutar.ParamByName('descricao').AsString := descricaolimpa; //Copy(qryProduto.FieldByName('descricao').AsString, 1, 75);
    v1 := 0;
    v2 := 0;
    v3 := 0;
    v4 := 0;
    if bal = '1' then
    begin
      v1 := Peso;
    end
    else
    begin
      if not Parametro.LIB_ATACADO_BALCAO then // alterado dia 20/08
      begin
        v1 := qryProduto.FieldByName('precovenda').AsFloat;
      end
      else
      begin
        if StrToFloatDef(Edit1.Text, 0) < Parametro.QUANT_PRODUTO then
        begin
          if selecao_qtd = 0 then
            v1 := qryProduto.FieldByName('precovenda').AsFloat
          else
            v1 := selecao_valor;
        end
        else
        begin
          v1 := qryProduto.FieldByName('atacado').AsFloat;
        end;
      end;
    end;

    // Teste lista de preços quando vende no Atacado  #VERIFICAR
    if Parametro.LIB_ATACADO then  //alterado em 20/08/22
    begin
      SetLength(ListPrecoAtacado, 3);
      ListPrecoAtacado[0].valor := qryProduto.FieldByName('PRECOVENDA').AsFloat;

      if not Parametro.DISPLAY_ATACADO then
        ListPrecoAtacado[0].Tipo := qryProduto.FieldByName('PRECOVENDA').DisplayLabel
      else
        ListPrecoAtacado[0].Tipo := 'CARTÃO / CRÉDITO';

      ListPrecoAtacado[1].valor := qryProduto.FieldByName('ATACADO').AsFloat;

      if not Parametro.DISPLAY_ATACADO then
        ListPrecoAtacado[1].Tipo := qryProduto.FieldByName('ATACADO').DisplayLabel
      else
        ListPrecoAtacado[1].Tipo := 'A VISTA / DÉBITO';

      ListPrecoAtacado[2].valor := qryProduto.FieldByName('PRECO3').AsFloat;
      ListPrecoAtacado[2].Tipo := qryProduto.FieldByName('PRECO3').DisplayLabel;

      v1 := GetPrecoAtacado(ListPrecoAtacado);
    end;
    // *** Fim teste

    if qryProduto.FieldByName('id').AsInteger = 0 then
      v1 := vProduto;

    v2 := (v1 / 100) * 85;
    v3 := v2 + ((v2 / 100) * 60);
    v4 := StrToFloatDef(Edit1.Text, 0);
    v5 := v1 * v4;
    q2 := 0;
    q3 := 0;
    V6 := 0;

    if bal = '1' then
    begin
      q2 := Peso;

      if selecao_qtd = 0 then
        q3 := qryProduto.FieldByName('precovenda').AsFloat
      else
        q3 := selecao_valor;

      if pesavel_kg = 1 then
        V6 := q2 * q3
      else
        V6 := q2 / q3; // q3/q2;
    end;

    // if bal = '1' then .ParamByName('d9').AsFloat:= v6 else .ParamByName('d9').AsFloat:=StrToFloatDef(edit1.Text);
    if bal = '1' then
    begin
      if pesavel_kg = 1 then
      begin
        qryExecutar.ParamByName('qtd').AsFloat := Peso;
      end
      else
      begin
        qryExecutar.ParamByName('qtd').AsFloat := V6;
      end;
    end
    else
    begin
      if StrIsFloat(Edit1.Text) = false then
      begin
        MessageDlg('QUANTIDADE DE PRODUTO INDEVIDA !', mtError, [mbOK], 0);
        exit;
        Edit1.SetFocus;
      end;

      if pacote = '0' then
      begin
        if selecao_qtd = 0 then
        begin
          qryExecutar.ParamByName('qtd').AsFloat := StrToFloatDef(Edit1.Text, 0);
          qryExecutar.ParamByName('qtdbaixa').AsFloat := 0;
        end
        else
        begin
          qryExecutar.ParamByName('qtd').AsFloat  := StrToFloatDef(Edit1.Text, 0) * selecao_qtd;
          qryExecutar.ParamByName('qtdbaixa').AsFloat := StrToFloatDef(Edit1.Text, 0) * selecao_qtd;
        end;
      end
      else
        qryExecutar.ParamByName('qtd').AsFloat := 10;
    end;
  end;

  if bal = '1' then
  begin
    if pesavel_kg = 1 then
    begin
      if selecao_qtd = 0 then
        qryExecutar.ParamByName('preco').AsFloat := qryProduto.FieldByName('precovenda').AsFloat // v6;
      else
        qryExecutar.ParamByName('preco').AsFloat := selecao_valor;
    end
    else
    begin
      qryExecutar.ParamByName('preco').AsFloat := Peso; // v1;
    end;
  end
  else
  begin
    if pacote = '0' then
    begin
      if selecao_qtd = 0 then
        qryExecutar.ParamByName('preco').AsFloat := v1
      else

        qryExecutar.ParamByName('preco').AsFloat := selecao_valor / selecao_qtd;    // aqui...
    end
    else
    begin
      qryExecutar.ParamByName('preco').AsFloat := qryProduto.FieldByName('atacado').AsFloat;
    end;
  end;

  qryExecutar.ParamByName('tributacao').AsString := '';
  qryExecutar.ParamByName('icms').AsFloat := v3;
  qryExecutar.ParamByName('iss').AsFloat := 0;
  qryExecutar.ParamByName('und').AsString := qryProduto.FieldByName('un_medida').AsString;
  qryExecutar.ParamByName('desconto').AsFloat := 0;
  qryExecutar.ParamByName('acrescimo').AsFloat := 0;

  if bal = '1' then
  begin
    if pesavel_kg = 1 then
      qryExecutar.ParamByName('total').AsFloat := RoundABNT( V6 , -2)
    else
      qryExecutar.ParamByName('total').AsFloat := Roundabnt( Peso, -2 ); // v1;
  end
  else
  begin
    if pacote = '0' then
    begin
      if selecao_qtd = 0 then
      begin
        qryExecutar.ParamByName('total').AsFloat := RoundABNT(v1 * StrToFloatDef(Edit1.Text, 0), -2)
      end
      else
      begin
        qryExecutar.ParamByName('total').AsFloat := Roundabnt((StrToFloat(Edit1.Text) * selecao_valor), -2);
      end;
    end
    else
    begin
      qryExecutar.ParamByName('total').AsFloat := Roundabnt(qryProduto.FieldByName('atacado').AsFloat * 10, -2);
    end;
  end;

  qryExecutar.ParamByName('serial').AsString := qryProduto.FieldByName('caracteristica').AsString;//atacado;
  qryExecutar.ParamByName('precocusto').AsFloat := qryProduto.FieldByName('precocusto').AsFloat;

  if qryProduto.FieldByName('id').AsInteger = 0 then
    qryExecutar.ParamByName('preco').AsFloat := vProduto;

  if bal = '1' then
    qryExecutar.ParamByName('preco').AsFloat := q3;

  qryExecutar.ExecSQL;
  // end;

  qryTMPItens.Close;
  qryTMPItens.SQL.Clear;
  qryTMPItens.SQL.add('select * from frente_tmpitvendas where cupom = :d0 and tipo = 1 order by cupom, item');
  qryTMPItens.ParamByName('d0').AsString := StaticText2.Caption;
  qryTMPItens.open;
  TNumericField(qryTMPItens.FieldByName('preco')).DisplayFormat := ',0.00;-,0.00';
  TNumericField(qryTMPItens.FieldByName('total')).DisplayFormat := ',0.00;-,0.00';

  contador := contador + 1;
  if Edit2.CanFocus then
    Edit2.SetFocus;

  selecao_qtd := 0;
  selecao_valor := 0;
end;

procedure TFbalcao.BitBtn13Click(Sender: TObject);
begin
  Panel20.Visible := false;
end;

procedure TFbalcao.Edit19Exit(Sender: TObject);
begin
  Edit19.Text := FormatFloat('0.00', StrToFloatDef(Edit19.Text, 0));
  ppLabel93.Caption := FormatFloat('0.00', StrToFloatDef(Edit19.Text, 0));
  ppLabel91.Caption := 'VALE GERADO EM: ' + DateTimeToStr(now);
  ppLabel2.Caption := 'No.: ' + Edit20.Text;
  ppLabel3.Caption := 'DATA DE EMISSÃO: ' + Label59.Caption;

  Vale.ShowPrintDialog := false;

  Vale.Print;

  BitBtn13.SetFocus;
end;

procedure TFbalcao.Edit20Exit(Sender: TObject);
begin
  Edit20.Text := AjustaStr_zero_esq(Edit20.Text, 6);

  qryConsulta.Close;
  qryConsulta.SQL.Clear;
  qryConsulta.SQL.add('select nota, emissao from vendas where nota = :d0 and cancelado = 0');
  qryConsulta.Params[0].AsString := AjustaStr_zero_esq(Edit20.Text, 6);
  qryConsulta.open;
  if qryConsulta.IsEmpty then
  begin
    ShowMessage('Cupom CANCELADO ou NÃO ENCONTRADO');
    Edit20.SetFocus;
    Edit20.Clear;
  end
  else
  begin
    Label59.Caption := qryConsulta.FieldByName('emissao').AsString;
    Edit19.SetFocus;
  end;

end;

procedure TFbalcao.BitBtn14Click(Sender: TObject);
var
  InputString: string;
begin
  PostMessage(Handle, InputBoxMessage, 0, 0);
  InputString := InputBox('Senha', 'Digite a senha', '');
  if InputString = parametro.senhad then
  begin
    if fimp = nil then
      fimp := Tfimp.Create(Application);

    RLPrinter.PrinterName := Parametro.impressora;

    fimp.RLReport10.PrintDialog := false;

    fimp.RLReport10.Print;

    fimp.Release;
    fimp := nil;
    Fbalcao.SetFocus;
  end
  else
  begin
    ShowMessage('Senha inválida');
  end;
end;

procedure TFbalcao.corbalcao;
begin

end;

procedure TFbalcao.BitBtn16Click(Sender: TObject);
begin
  Panel21.Visible := false;
  Edit2.Clear;
  if Edit2.CanFocus then
    Edit2.SetFocus;
end;

procedure TFbalcao.Edit21Exit(Sender: TObject);
begin
  if (Edit21.Text = '1') or (Edit21.Text = '2') then
  begin
    BitBtn15.SetFocus;
  end
  else
  begin
    ShowMessage('Apenas permitido valor 1 e 2');
    Edit21.Clear;
    Edit21.SetFocus;
  end;
end;

procedure TFbalcao.BitBtn15Click(Sender: TObject);
begin
  if not qryTMPItens.IsEmpty then
  begin
    salva_Itens_temp;

    atualiza;

    Edit2.Clear;
    Label47.Caption := '';
    Edit1.Text := '0001';
  end;
  Panel21.Visible := false;
  if Edit2.CanFocus then
    Edit2.SetFocus;
end;

function TFbalcao.StrToPaginaCodigo(const AValor: String): TACBrPosPaginaCodigo;
begin
  result := TACBrPosPaginaCodigo(GetEnumValue(TypeInfo(TACBrPosPaginaCodigo), AValor));
end;

function TFbalcao.PathApp: String;
var
  pasta : string;
begin
  if TipoDocAtual=tdCFe then
    pasta := 'CFe'
  else
    pasta := 'NFCe';

  result := IncludeTrailingPathDelimiter(ExtractFilePath(ParamStr(0)) + pasta);
  if not DirectoryExists(result) then
    ForceDirectories(result);
end;

function TFbalcao.PathLog: String;
begin
  result := IncludeTrailingPathDelimiter(PathApp + 'Log');
  if not DirectoryExists(result) then
    ForceDirectories(result);
end;

procedure TFbalcao.DiretoriosDeArquivos;
begin
  // somente para NFC-e
  PathSchemas := IncludeTrailingPathDelimiter(PathApp + 'Schemas');

  // caminhos de pastas especificos por cnpj e comuns aos dois modos de funcionamento
  PathArqDFe := IncludeTrailingPathDelimiter(PathApp + 'Documentos');
  PathPDF := IncludeTrailingPathDelimiter(PathArqDFe + 'PDF');
  PathArquivos := IncludeTrailingPathDelimiter(PathArqDFe + 'Arquivos');
  PathTmp := IncludeTrailingPathDelimiter(PathArqDFe + 'Tmp');

  if not DirectoryExists(PathPDF) then
    ForceDirectories(PathPDF);

  if not DirectoryExists(PathArquivos) then
    ForceDirectories(PathArquivos);

  if not DirectoryExists(PathTmp) then
    ForceDirectories(PathTmp);

  if not DirectoryExists(PathLog) then
    ForceDirectories(PathLog);

  // Geração de logs
  DM1.AcbrSAT1.ArqLOG := PathLog + formatDateTime('"SAT_"yyyymmdd".TXT"', date);
  { ACBrIntegrador1.ArqLOG := PathLog + FormatDateTime('"INTEGRADOR_"yyyymmdd".TXT"', DATE); }
  DM1.ACBrPosPrinter1.ArqLOG := PathLog + formatDateTime('"POSPRINTER_"yyyymmdd".TXT"', date);
end;

procedure TFbalcao.ConfiguraImpressora(Tipo: String);
begin
  DM1.qryTerminal.Locate('nome', DM1.NomeTerminal, []);

  vImprime := DM1.qryTerminalIMPRIME.Value = 'S';

  if (Tipo = '1') and (TipoDocAtual=tdCFe) then    // 1 = Papel A4  | 2 = Bobina
  begin
    DM1.AcbrSAT1.Extrato := DM1.ACBrSATExtratoFortes1;
    DM1.ACBrSATExtratoFortes1.impressora  := DM1.qryTerminalPorta.AsString;

//    if parametro.IMPRESSORA_SAT <> '' then
//    begin
//      DM1.ACBrSATExtratoFortes1.impressora := Label30.Caption;
//    end
//    else
//    begin
//      DM1.ACBrSATExtratoFortes1.impressora := Parametro.IMPRESSORA_SAT;
//    end;

  end;

  // 1 = A4
  // 2 = EscoPos

  if (Tipo = '2') and (TipoDocAtual=tdCFe) then
  begin
    DM1.AcbrSAT1.Extrato := DM1.ACBrSATExtratoESCPOS1;
    DM1.ACBrSATExtratoESCPOS1.impressora := DM1.qryTerminalPorta.AsString;
  end;

//  if (Tipo = '1') and (TipoDocAtual=tdCFe) then
//  begin
//    DM1.AcbrSAT1.Extrato := DM1.ACBrSATExtratoFortes1;
//    DM1.ACBrSATExtratoFortes1.impressora := DM1.qryTerminalPorta.AsString;
//  end;


  if (Tipo = '1') and (TipoDocAtual=tdNFCe) then
  begin
    DM1.ACBrNFe1.DANFE           := DM1.ACBrNFeDANFCEFR1;
    DM1.ACBrNFe1.DANFE.PathPDF   := DM1.qryConfigPATHPDF.Value;
    DM1.ACBrNFe1.DANFE.NumCopias := 1;
    if DM1.qryConfigNCOPIAS.AsInteger > 1 then
      DM1.ACBrNFe1.DANFE.NumCopias := DM1.qryConfigNCOPIAS.Value;
    DM1.ACBrNFe1.DANFE.LOGO := DM1.qryConfigLOGOMARCA.Value;
    dm1.ACBrNFeDANFCEFR1.Impressora := DM1.qryTerminalPorta.AsString;
  end;

  if (Tipo = '2') {and (TipoDocAtual=tdNFCe)} then
  begin
    DM1.ACBrPosPrinter1.Desativar;

    DM1.ACBrNFe1.DANFE := DM1.ACBrNFeDANFeESCPOS1;
    DM1.ACBrNFe1.DANFE.NumCopias := 1;
    if DM1.qryConfigNCOPIAS.AsInteger > 1 then
      DM1.ACBrNFe1.DANFE.NumCopias := DM1.qryConfigNCOPIAS.Value;

    DM1.ACBrNFe1.DANFE.PathPDF := DM1.qryConfigPATHPDF.Value;
    DM1.ACBrNFe1.DANFE.LOGO := DM1.qryConfigLOGOMARCA.Value;

    if DM1.qryTerminal.FieldByName('MODELO').Value = 'DARUMA' then
      DM1.ACBrPosPrinter1.Modelo := ppEscDaruma
    else
      if DM1.qryTerminal.FieldByName('MODELO').Value = 'BEMATECH' then
        DM1.ACBrPosPrinter1.Modelo := ppEscBematech
      else
        if DM1.qryTerminal.FieldByName('MODELO').Value = 'ELGIN' then
          DM1.ACBrPosPrinter1.Modelo := ppEscPosEpson
        else
          if DM1.qryTerminal.FieldByName('MODELO').Value = 'DIEBOLD' then
            DM1.ACBrPosPrinter1.Modelo := ppEscDiebold
          else
            if DM1.qryTerminal.FieldByName('MODELO').Value = 'EPSON' then
              DM1.ACBrPosPrinter1.Modelo := ppEscPosEpson
            else
              if DM1.qryTerminal.FieldByName('MODELO').Value = 'VOX' then
                DM1.ACBrPosPrinter1.Modelo := ppEscVox
              else
                if DM1.qryTerminal.FieldByName('MODELO').Value = 'POSSTAR' then
                  DM1.ACBrPosPrinter1.Modelo := ppEscPosStar
                else
                  if DM1.qryTerminal.FieldByName('MODELO').Value = 'JIANG' then
                    DM1.ACBrPosPrinter1.Modelo := ppEscZJiang
                  else
                    if DM1.qryTerminal.FieldByName('MODELO').Value = 'GPRINTER' then
                      DM1.ACBrPosPrinter1.Modelo := ppEscGPrinter
                    else
                      if DM1.qryTerminal.FieldByName('MODELO').Value = 'EPSONP2' then
                        DM1.ACBrPosPrinter1.Modelo := ppEscEpsonP2
                      else
                        DM1.ACBrPosPrinter1.Modelo := ppTexto;

    DM1.ACBrPosPrinter1.porta := LowerCase(DM1.qryTerminal.FieldByName('PORTA').AsString);
    DM1.ACBrPosPrinter1.Device.Baud := DM1.qryTerminalVELOCIDADE.Value;
    DM1.ACBrPosPrinter1.PaginaDeCodigo := StrToPaginaCodigo(DM1.qryTerminalPAGINA_CODIGO.AsString);

    DM1.ACBrPosPrinter1.Ativar;
  end;

  DM1.ACBrPosPrinter1.EspacoEntreLinhas := StrToIntDef(DM1.qryTerminalESPACO_ENTRE_LINHAS.Value, 0);
  DM1.ACBrPosPrinter1.LinhasEntreCupons := StrToIntDef(DM1.qryTerminalLINHAS_ENTRE_CUPOM.Value, 0);

  DM1.ACBrNFeDANFeESCPOS1.LarguraBobina := DM1.qryTerminalLARGURA_BOBINA.AsInteger;
  DM1.ACBrNFeDANFeESCPOS1.MARGEMDIREITA := DM1.qryTerminalMARGEM_DIREITA.AsFloat;
  DM1.ACBrNFeDANFeESCPOS1.MARGEMESQUERDA := DM1.qryTerminalMARGEM_ESQUERDA.AsFloat;
  DM1.ACBrNFeDANFeESCPOS1.MargemInferior := DM1.qryTerminalMARGEM_INFERIOR.AsFloat;
  DM1.ACBrNFeDANFeESCPOS1.MargemSuperior := DM1.qryTerminalMARGEM_SUPERIOR.AsFloat;

  DM1.ACBrNFeDANFeESCPOS1.ImprimeEmUmaLinha := false;
  if DM1.qryTerminalIMPRIME_DUAS_LINHAS.Value = 'S' then
    DM1.ACBrNFeDANFeESCPOS1.ImprimeEmUmaLinha := true;

  DM1.ACBrNFeDANFeESCPOS1.Sistema := 'IN9VE SISTEMAS'; // #verificar

  DM1.ACBrNFeDANFeRL1.Sistema := 'IN9VE SISTEMAS'; // #verificar
  DM1.ACBrNFeDANFeRL1.Site := '';

  DM1.ACBrNFeDANFCeFortesA41.Sistema := 'IN9VE SISTEMAS';
end;

procedure TFbalcao.GetsignAC(var Chave: AnsiString);
begin
  Chave := AnsiString(DM1.qryConfigCODIGO_VINCULACAO.AsString);
end;

procedure TFbalcao.GetcodigoDeAtivacao(var Chave: AnsiString);
begin
  Chave := AnsiString(DM1.qryConfigCODIGO_ATIVACAO.AsString);
end;

procedure TFbalcao.ConfiguraSAT;
var
  ModeloSAT: TSatModelo;
begin
  // #verificar

  DM1.qryConsulta.Close;
  DM1.qryConsulta.SQL.Text := 'select CONTINGENCIA,PORTA,MODELO,NVIAS,IMPRIME,USAGAVETA,VELOCIDADE ' + 'from VENDAS_TERMINAIS ' + 'where NOME=' + QuotedStr(DM1.Getcomputer);
  DM1.qryConsulta.open;

  DiretoriosDeArquivos;

  DM1.qryConfig.Close;
  DM1.qryConfig.open;

  if DM1.qryConfig.IsEmpty then
    raise Exception.Create('Módulo DF-e ainda não foi configurado, impossível continuar!');

  DM1.AcbrSAT1.DesInicializar;
  if not DM1.qryConfig.IsEmpty then
  begin
    ModeloSAT := DM1.GetModeloSAT(DM1.qryConfigMODELO_DLL.AsString);

    { if ModeloSAT.Tipo = mfe_Integrador_XML then
      dm1.ACBrSAT1.Integrador := ACBrIntegrador1; }

    DM1.AcbrSAT1.OnGetsignAC := GetsignAC;
    DM1.AcbrSAT1.OnGetcodigoDeAtivacao := GetcodigoDeAtivacao;
    DM1.AcbrSAT1.Modelo := ModeloSAT.Tipo;
    DM1.AcbrSAT1.NomeDLL := ModeloSAT.PathDll;
    DM1.AcbrSAT1.Config.ide_numeroCaixa := StrToInt(DM1.TERMINAL);
    DM1.AcbrSAT1.Config.ide_CNPJ := TiraPontos(DM1.qryConfigSAT_CNPJ.AsString);
    DM1.AcbrSAT1.Config.emit_CNPJ := TiraPontos(GEmitente.CPFCNPJ);
    DM1.AcbrSAT1.Config.emit_IE := TiraPontos(GEmitente.InscRG);
    DM1.AcbrSAT1.Config.emit_IM := TiraPontos(GEmitente.InscMunicipal);
    DM1.AcbrSAT1.Config.emit_cRegTribISSQN := RTISSMicroempresaMunicipal;
    DM1.AcbrSAT1.Config.emit_indRatISSQN := irSim;
    DM1.AcbrSAT1.Config.PaginaDeCodigo := ModeloSAT.PaginaCodigo;
    DM1.AcbrSAT1.Config.EhUTF8 := ModeloSAT.UTF8;
    DM1.AcbrSAT1.Config.infCFe_versaoDadosEnt := StrToFloatDef(DM1.qryConfigCFE_VERSAO.AsString, 0.07);

    if Trim(GEmitente.Regime) = 'SIMPLES NACIONAL' then
      DM1.AcbrSAT1.Config.emit_cRegTrib := RTSimplesNacional
    else
      DM1.AcbrSAT1.Config.emit_cRegTrib := RTRegimeNormal;

    DM1.AcbrSAT1.ConfigArquivos.SalvarCFe := true;
    DM1.AcbrSAT1.ConfigArquivos.SalvarCFeCanc := true;
    DM1.AcbrSAT1.ConfigArquivos.SalvarEnvio := false;
    DM1.AcbrSAT1.ConfigArquivos.SepararPorCNPJ := true;
    DM1.AcbrSAT1.ConfigArquivos.SepararPorMes := true;

    // Diretorios onde salvar os arquivos
    DM1.AcbrSAT1.ConfigArquivos.PastaCFeVenda := PathArquivos;
    DM1.AcbrSAT1.ConfigArquivos.PastaCFeCancelamento := PathArquivos;
    DM1.AcbrSAT1.ConfigArquivos.PastaEnvio := PathTmp;
    DM1.AcbrSAT1.CFe.IdentarXML := false;
    DM1.AcbrSAT1.CFe.TamanhoIdentacao := 1;

    DM1.AcbrSAT1.ConfigArquivos.SalvarCFe := true;
    DM1.AcbrSAT1.ConfigArquivos.SalvarCFeCanc := true;
    DM1.AcbrSAT1.ConfigArquivos.SalvarEnvio := true;

    // Rede
    if (Trim(DM1.qryConfigSAT_USUARIO.AsString) <> '') or (Trim(DM1.qryConfigSAT_LANIP.AsString) <> '') then
    begin
      DM1.AcbrSAT1.Rede.tipoInter := TTipoInterface(DM1.qryConfigSAT_TIPO_INTER.AsInteger);
      DM1.AcbrSAT1.Rede.SSID := DM1.qryConfigSAT_SSID.AsString;
      DM1.AcbrSAT1.Rede.seg := TSegSemFio(DM1.qryConfigSAT_SEG.AsInteger);
      DM1.AcbrSAT1.Rede.codigo := DM1.qryConfigSAT_CODIGO.AsString;
      DM1.AcbrSAT1.Rede.tipoLan := TTipoLan(DM1.qryConfigSAT_TIPOLAN.AsInteger);
      DM1.AcbrSAT1.Rede.lanIP := DM1.qryConfigSAT_LANIP.AsString;
      DM1.AcbrSAT1.Rede.lanMask := DM1.qryConfigSAT_LANMASK.AsString;
      DM1.AcbrSAT1.Rede.lanGW := DM1.qryConfigSAT_LANGW.AsString;
      DM1.AcbrSAT1.Rede.lanDNS1 := DM1.qryConfigSAT_LANDNS1.AsString;
      DM1.AcbrSAT1.Rede.lanDNS2 := DM1.qryConfigSAT_LANDNS2.AsString;
      DM1.AcbrSAT1.Rede.usuario := DM1.qryConfigSAT_USUARIO.AsString;
      DM1.AcbrSAT1.Rede.senha := DM1.qryConfigSAT_SENHA.AsString;
      DM1.AcbrSAT1.Rede.proxy := DM1.qryConfigSAT_PROXY.AsInteger;
      DM1.AcbrSAT1.Rede.proxy_ip := DM1.qryConfigSAT_PROXY_IP.AsString;
      DM1.AcbrSAT1.Rede.proxy_porta := DM1.qryConfigSAT_PROXY_PORTA.AsInteger;
      DM1.AcbrSAT1.Rede.proxy_user := DM1.qryConfigSAT_PROXY_USER.AsString;
      DM1.AcbrSAT1.Rede.proxy_senha := DM1.qryConfigSAT_PROXY_SENHA.AsString;
    end;

    DM1.AcbrSAT1.Inicializar;
  end;

  DM1.qryTerminal.Close;
  DM1.qryTerminal.open;
  DM1.qryTerminal.Locate('nome', DM1.NomeTerminal, []);

  // configurações impressão escpos
 { if DM1.qryTerminalTIPOIMPRESSORA.AsString = '1' then    // 1 = Papel A4  | 2 = Bobina
  begin
    DM1.AcbrSAT1.Extrato := DM1.ACBrSATExtratoFortes1;
    if parametro.IMPRESSORA_SAT <> '' then
    begin
      DM1.ACBrSATExtratoFortes1.impressora := Label30.Caption;
    end
    else
    begin
      DM1.ACBrSATExtratoFortes1.impressora := Parametro.IMPRESSORA_SAT;
    end;
  end
  else
  begin
    DM1.AcbrSAT1.Extrato := DM1.ACBrSATExtratoESCPOS1;
    if parametro.IMPRESSORA_SAT <> '' then
    DM1.ACBrSATExtratoESCPOS1.impressora := Parametro.IMPRESSORA_SAT;
    ConfiguraImpressora('');
  end;
  }

  ConfiguraImpressora(DM1.qryTerminalTIPOIMPRESSORA.AsString);

  DM1.AcbrSAT1.Extrato.Sistema := 'IN9VE SISTEMAS'; // #verificar
  DM1.AcbrSAT1.Extrato.ImprimeEmUmaLinha := false;
  DM1.AcbrSAT1.Extrato.PathPDF := PathPDF;
  DM1.AcbrSAT1.Extrato.ImprimeDescAcrescItem := true;
  DM1.AcbrSAT1.Extrato.ImprimeCodigoEan := false;

  if FilesExists(DM1.qryConfigLOGOMARCA.AsString) then
    DM1.AcbrSAT1.Extrato.LOGO := DM1.qryConfigLOGOMARCA.AsString;

  DM1.AcbrSAT1.Extrato.Site := ''; // dm1.qryEmpresaSITE.AsString;   #verificar
  DM1.AcbrSAT1.Extrato.Email := ''; // dm1.qryEmpresaEMAIL.AsString;
  DM1.AcbrSAT1.Extrato.MostraPreview := DM1.ACBrSATExtratoFortes1.impressora.IsEmpty;

  if Label9.Caption <> '0' then
  begin
    if Parametro.NP then
    begin
      iF MessageDlg('IMPRIMIR NOTA PROMISSÓRIA?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
        imprimir_np;
    end;
  end;
end;

procedure TFbalcao.GerarCFe(const ASerie, ANumero: Integer);
var
  NumItem: Integer;
  vOK: Boolean;
  CodigoGTIN: String;
  MsgErroGTIN: String;
begin
  DM1.qryNFCE_M.Close;
  DM1.qryNFCE_M.Params[0].Value := StrToInt(StaticText2.Caption); // qryVendasNOTA.Value;
  DM1.qryNFCE_M.open;

  DM1.qryNFCE_D.Close;
  DM1.qryNFCE_D.Params[0].Value := DM1.qryNFCE_MCODIGO.Value;
  DM1.qryNFCE_D.open;

  if DM1.qryNFCE_D.IsEmpty then
  begin
    ShowMessage('Não Existe Produto Cadastrado Para Venda!' + #13 + 'Vá na tela cadastro de produtos' + #13 + ' e marque a opção Permitir Venda');
    exit;
  end;

  // Verifica atualiza cadastro de produtos
  try

    DM1.AcbrSAT1.InicializaCFe;

    // Montando uma Venda //
    with DM1.AcbrSAT1.CFe do
    begin
      ide.numeroCaixa := StrToInt(DM1.TERMINAL);

      // dados do cliente
      if qryVendasCLIENTE.AsInteger <> DM1.qryConfigCLIENTE_PADRAO.AsInteger then
      begin
        DM1.qryConsulta.Close;
        DM1.qryConsulta.SQL.Text := 'SELECT NOME FROM PESSOA WHERE ID=:ID';
        DM1.qryConsulta.Params[0].Value := DM1.qryNFCE_MID_CLIENTE.Value;
        DM1.qryConsulta.open;

        Dest.CNPJCPF := TiraPontos(DM1.qryNFCE_MCPF_NOTA.AsString);
        Dest.xNome := DM1.qryConsulta.FieldByName('NOME').AsString;
      end
      else
      begin
        if Length(TiraPontos(DM1.qryNFCE_MCPF_NOTA.AsString)) >= 9 then
          Dest.CNPJCPF := TiraPontos(DM1.qryNFCE_MCPF_NOTA.AsString)
        else
          if Length(TiraPontos(qryVendasCPF_NOTA.Value)) >= 9 then
            Dest.CNPJCPF := TiraPontos(qryVendasCPF_NOTA.Value)
          else
            Dest.CNPJCPF := '';
        Dest.xNome := 'CONSUMIDOR FINAL';
      end;

      // endereço de entrega
      Entrega.xLgr := '';
      Entrega.nro := '';
      Entrega.xCpl := '';
      Entrega.xBairro := '';
      Entrega.xMun := '';
      Entrega.UF := '';

      // itens da venda
      NumItem := 0;

      DM1.qryNFCE_D.First;
      while not DM1.qryNFCE_D.Eof do
      begin
        NumItem := NumItem + 1;

        with Det.add do
        begin
          CodigoGTIN := Trim(OnlyNumber(DM1.QRYNFCE_DCOD_BARRA.AsString));
          if CodigoGTIN <> '' then
          begin
            DM1.ACBrValidador1.TipoDocto := docGTIN;
            DM1.ACBrValidador1.Documento := CodigoGTIN;
            if not DM1.ACBrValidador1.Validar then
              CodigoGTIN := '';
          end
          else
            CodigoGTIN := '';

          nItem := NumItem;
          Prod.cProd := DM1.qryNFCE_DID_PRODUTO.AsString;
          Prod.cEAN := CodigoGTIN;
          Prod.xProd := DM1.qryNFCE_DDESCRICAO.AsString;
          Prod.NCM := DM1.qryNFCE_DNCM.AsString;
          Prod.CFOP := DM1.qryNFCE_DCFOP.AsString;
          Prod.uCom := DM1.QRYNFCE_DUNIDADE.AsString;
          Prod.indRegra := irArredondamento;
          Prod.qCom := DM1.QRYNFCE_DQTD.AsFloat; //Trunc(Total/vunit);
          Prod.vUnCom := RoundABNT(DM1.QRYNFCE_DPRECO.AsFloat, -2);
          Prod.vDesc := RoundABNT(DM1.QRYNFCE_DVDESCONTO.AsFloat, -2);
          Prod.vOutro := RoundABNT(DM1.qryNFCE_DOUTROS.AsFloat, -2);
          Prod.CEST := '';

          if DM1.qryNFCE_DCFOP.AsString = '5656' then
          begin
            prod.EhCombustivel := True;

            with Prod.obsFiscoDet.Add do
            begin
              xCampoDet := 'Cod. Produto ANP';
              xTextoDet := '320101001';
            end;
          end;

          // observações do produto
          infAdProd := '';

          // ICMS ********************************************************
          Imposto.ICMS.orig := StrToOrig(vOK, Copy(DM1.QRYNFCE_DCST.Value, 1, 1));
          if not vOK then
          begin
            raise Exception.CreateFmt('Código origem "%d" inválida para o item "%s - %s"', [Copy(DM1.QRYNFCE_DCST.Value, 1, 1), DM1.qryNFCE_DID_PRODUTO.AsString, DM1.qryNFCE_DDESCRICAO.AsString]);
          end;

          if DM1.AcbrSAT1.Config.emit_cRegTrib = RTRegimeNormal then
          begin
            Imposto.ICMS.CST := StrToCSTICMS(vOK, Copy(DM1.QRYNFCE_DCST.AsString, 2, 2));
            if not vOK then
            begin
              raise Exception.CreateFmt('Código do CST "%s" inválido para o item "%s - %s"', [Copy(DM1.QRYNFCE_DCST.AsString, 2, 2), DM1.qryNFCE_DID_PRODUTO.AsString, DM1.qryNFCE_DDESCRICAO.AsString]);
            end;

            Imposto.ICMS.pICMS := DM1.QRYNFCE_DALIQ_ICMS.AsFloat;
          end
          else
          begin
            Imposto.ICMS.CSOSN := StrToCSOSNIcms(vOK, DM1.QRYNFCE_DCSOSN.AsString);
            if not vOK then
            begin
              raise Exception.CreateFmt('Código do CSOSN "%s" inválido para o item "%s - %s"', [DM1.QRYNFCE_DCSOSN.AsString, DM1.qryNFCE_DID_PRODUTO.AsString, DM1.qryNFCE_DDESCRICAO.AsString]);
            end;
          end;

          // PIS *********************************************************
          with Imposto.PIS do
          begin
            if (Imposto.ICMS.CSOSN = csosn500) or (DM1.AcbrSAT1.Config.emit_cRegTrib = RTSimplesNacional) then
              CST := pis49
            else
              CST := StrToCSTPIS(vOK, DM1.QRYNFCE_DCST_PIS.AsString);

            if not vOK then
            begin
              raise Exception.CreateFmt('Código CST do Pis "%d" inválido para o item "%s - %s"', [DM1.QRYNFCE_DCST_PIS.AsString, DM1.qryNFCE_DID_PRODUTO.AsString, DM1.qryNFCE_DDESCRICAO.AsString]);
            end;

            if DM1.QRYNFCE_DALIQ_PIS_ICMS.AsFloat > 0 then
            begin
              vBC := DM1.QRYNFCE_DBASE_PIS_ICMS.AsFloat;
              pPIS := DM1.QRYNFCE_DALIQ_PIS_ICMS.AsFloat / 100;
              qBCProd := 0;
              vAliqProd := 0;
            end
            else
            begin
              vBC := 0;
              pPIS := 0;
              qBCProd := 0;
              vAliqProd := 0;
            end;
          end;

          // COFINS ******************************************************
          with Imposto.COFINS do
          begin
            if (Imposto.ICMS.CSOSN = csosn500) or (DM1.AcbrSAT1.Config.emit_cRegTrib = RTSimplesNacional) then
              CST := cof49
            else
              CST := StrToCSTCOFINS(vOK, DM1.QRYNFCE_DCST_COFINS.AsString);

            if not vOK then
            begin
              raise Exception.CreateFmt('Código CST do Cofins "%d" inválido para o item "%s - %s"', [DM1.QRYNFCE_DCST_COFINS.AsString, DM1.qryNFCE_DID_PRODUTO.AsString, DM1.qryNFCE_DDESCRICAO.AsString]);
            end;

            if DM1.QRYNFCE_DALIQ_COFINS_ICMS.AsFloat > 0 then
            begin
              vBC := DM1.QRYNFCE_DBASE_COFINS_ICMS.AsFloat;
              pCOFINS := DM1.QRYNFCE_DALIQ_COFINS_ICMS.AsFloat / 100;

              qBCProd := 0;
              vAliqProd := 0;
            end
            else
            begin
              vBC := 0;
              pCOFINS := 0;
              qBCProd := 0;
              vAliqProd := 0;
            end;
          end;

          // imposto aproximado
          Imposto.vItem12741 := DM1.QRYNFCE_DTRIB_FED.AsFloat + DM1.QRYNFCE_DTRIB_EST.AsFloat + DM1.QRYNFCE_DTRIB_MUN.AsFloat;
        end;

        DM1.qryNFCE_D.Next;
      end;

      Total.DescAcrEntr.vAcresSubtot := DM1.qryNFCE_MOUTROS.AsFloat;
      //Total.DescAcrEntr.vDescSubtot := DM1.qryNFCE_MDESCONTO.AsFloat;

      Total.vCFeLei12741 := DM1.qryNFCE_MTRIB_FED.AsFloat + DM1.qryNFCE_MTRIB_EST.AsFloat + DM1.qryNFCE_MTRIB_MUN.AsFloat;

      { qryRecVendas.Close;
        qryRecVendas.Params[0].Value := StrToInt(StaticText2.Caption);
        qryRecVendas.Open; }

//      if not dm1.cdsPagto.Active then
//      dm1.cdsPagto.Open;

      if ComboBox1.ItemIndex <> 0 then
      begin
          ACBrSAT1.CFe.Pagto.vTroco := rtroco;

          with Pagto.add do
          begin
            cMP := mpOutros;
            vMP := RoundABNT(DM1.qryNFCE_MTOTAL.AsFloat, -2);   //roundTo
          end;
      end
      else
      begin
      DM1.cdsPagto.First;

//      if ( DM1.qryNFCE_MTOTAL.Value = qryVendasTOTAL.AsFloat { and ( not qryRecVendas.IsEmpty) } then
      if ( RoundABNT(DM1.qryNFCE_MTOTAL.Value, -2) = RoundABNT(qryVendasTOTAL.AsFloat,-2)) then
      begin
        if Parametro.FISCAL_BAIXA then        //not
          begin
            while not DM1.cdsPagto.Eof do
            begin
              if DM1.cdsPagtoVALOR.Value > 0 then
              begin
                if UpperCase(DM1.cdsPagtoTIPO.Value) = 'D' then
                begin
                  with Pagto.add do
                  begin
                    cMP := mpDinheiro;
                    vMP := RoundABNT(DM1.cdsPagtoVALOR.AsFloat, -2);   //era assim RoundTo(DM1.cdsPagtoVALOR.AsFloat, -2);
                  end;
                end;
              end;

              if UpperCase(DM1.cdsPagtoTIPO.Value) = 'Q' then
              begin
                if DM1.cdsPagtoVALOR.Value > 0 then
                begin
                  with Pagto.add do
                  begin
                    cMP := mpCheque;
                    vMP := RoundABNT(DM1.cdsPagtoVALOR.AsFloat, -2);
                  end;
                end;
              end;

              if UpperCase(DM1.cdsPagtoTIPO.Value) = 'T' then
              begin
                if DM1.cdsPagtoVALOR.Value > 0 then
                begin
                  with Pagto.add do
                  begin
                    cMP := mpOutros;
                    vMP := RoundABNT(DM1.cdsPagtoVALOR.AsFloat, -2);
                  end;
                end;
              end;

              if UpperCase(DM1.cdsPagtoTIPO.Value) = 'C' then
              begin
                if DM1.cdsPagtoVALOR.Value > 0 then
                begin
                  with Pagto.add do
                  begin
                    cMP := mpCartaodeCredito;
                    vMP := RoundABNT(DM1.cdsPagtoVALOR.AsFloat, -2);
                  end;
                end;
              end;
              if UpperCase(DM1.cdsPagtoTIPO.Value) = 'E' then
              begin
                if RoundABNT(DM1.cdsPagtoVALOR.Value, -2) > 0 then
                begin
                  with Pagto.add do
                  begin
                    cMP := mpCartaodeDebito;
                    vMP := RoundABNT(DM1.cdsPagtoVALOR.AsFloat, -2);
                  end;
                end;
              end;

              if UpperCase(DM1.cdsPagtoTIPO.Value) = 'L' then
              begin
                if DM1.cdsPagtoVALOR.Value > 0 then
                begin
                  with Pagto.add do
                  begin
                    cMP := mpCreditoLoja;
                    vMP := RoundTo(DM1.cdsPagtoVALOR.AsFloat, -2);
                  end;
                end;
              end;

              if UpperCase(DM1.cdsPagtoTIPO.Value) = 'O' then
              begin
                if RoundABNT(DM1.cdsPagtoVALOR.Value, -2) > 0 then
                begin
                  with Pagto.add do
                  begin
                    cMP := mpOutros;
                    vMP := RoundABNT(DM1.cdsPagtoVALOR.AsFloat, -2);
                  end;
                end;
              end;

              DM1.cdsPagto.Next;
            end;
          end
          else
          begin
            with Pagto.add do
            begin
              cMP := mpDinheiro;
              vMP := RoundABNT(DM1.qryNFCE_MTOTAL.AsFloat, -2);
            end;
          end;
        end
        else
        begin
          with Pagto.add do
          begin
            cMP := mpDinheiro;
            vMP := RoundABNT(DM1.qryNFCE_MTOTAL.AsFloat, -2);
          end;
        end;
      end;
      ACBrSAT1.CFe.Pagto.vTroco := rtroco;
    end;
  except
    on e: Exception do
      raise Exception.Create(e.Message);
  end;
end;

procedure TFbalcao.GerarNFCe(const ASerie, ANumero: Integer);
var
  NumItem: Integer;
  vOK: Boolean;
  CodigoGTIN: String;
  MsgErroGTIN: String;
  xobs: string;
begin
  DM1.qryNFCE_M.Close;
  DM1.qryNFCE_M.Params[0].Value := StrToInt(StaticText2.Caption); // qryVendasNOTA.Value;
  DM1.qryNFCE_M.Open;

  DM1.qryNFCE_D.Close;
  DM1.qryNFCE_D.Params[0].Value := DM1.qryNFCE_MCODIGO.Value;
  DM1.qryNFCE_D.Open;

  if DM1.qryNFCE_D.IsEmpty then
  begin
    ShowMessage('Não Existe Produto Cadastrado Para Venda!' + #13 + 'Vá na tela cadastro de produtos' + #13 + ' e marque a opção Permitir Venda');
    Exit;
  end;

  try
    // Inicializa a NFC-e
    DM1.ACBrNFe1.NotasFiscais.Clear;

    with DM1.ACBrNFe1.NotasFiscais.Add.NFe do
    begin
      if DM1.qryConfigAMBIENTE.AsInteger = 0 then
        ide.tpAmb := taProducao
        else
        ide.tpAmb := taHomologacao;

      ide.cMunFG := GEmitente.codigo_cidade;
      ide.modelo := 65; // Modelo da NFC-e
      Ide.dEmi := now;
      ide.serie := ASerie;
      ide.nNF := ANumero;
      ide.cNF := qryVendasNOTA.AsInteger; // Função que gera um código numérico aleatório
      ide.natOp := 'Venda de Mercadoria'; // Natureza da Operação
      ide.indFinal := cfConsumidorFinal; // Indica que é venda ao consumidor final
      ide.indPres := pcPresencial; // Operação presencial
      ide.procEmi := peAplicativoContribuinte; // Tipo de emissão
      ide.cUF := GEmitente.codigo_uf;
      Ide.tpImp := tiNFCe;       // acbr 709 rejeicao

      // Dados do Emitente
      if PARAMETRO.RAZAOSOCIAL <> '0' then
        Emit.xNome := Parametro.RAZAOSOCIAL
      else
        Emit.xNome := GEmitente.RazaoSocial;

      Emit.CNPJCPF := TiraPontos(GEmitente.CPFCNPJ);
      Emit.IE      := TiraPontos(GEmitente.InscRG);
      Emit.xFant   := GEmitente.Fantasia;

      Emit.EnderEmit.fone := GEmitente.Telefone;
      Emit.EnderEmit.CEP  := StrToInt(tirapontos(GEmitente.CEP));
      Emit.EnderEmit.xLgr := GEmitente.Endereco;
      Emit.EnderEmit.nro  := GEmitente.Numero;
      Emit.EnderEmit.xCpl := GEmitente.Complemento;
      Emit.EnderEmit.xBairro := GEmitente.Bairro;
      Emit.EnderEmit.cMun := GEmitente.codigo_cidade; // 1302603; //GEmitente.codigo_cidade;
      Emit.EnderEmit.xMun := GEmitente.Cidade;
      Emit.EnderEmit.UF   := GEmitente.UF;

      Emit.EnderEmit.cPais := 1058;
      Emit.EnderEmit.xPais := 'BRASIL';

      Emit.IEST := '';
      Emit.IM := GEmitente.InscMunicipal;
      Emit.CNAE := tirapontos(GEmitente.CNAE);

      if trim(GEmitente.Regime) = 'SIMPLES NACIONAL' then
        Emit.CRT := crtSimplesNacional
      else
        Emit.CRT := crtRegimeNormal;

      // Dados do Destinatário
//      if qryVendasCLIENTE.AsInteger <> DM1.qryConfigCLIENTE_PADRAO.AsInteger then MUDADO EM 28/11/2024 FABIANO
      if qryVendasCLIENTE.AsInteger = DM1.qryConfigCLIENTE_PADRAO.AsInteger then
      begin
        DM1.qryConsulta.Close;
        DM1.qryConsulta.SQL.Text := 'SELECT NOME FROM PESSOA WHERE ID=:ID';
        DM1.qryConsulta.Params[0].Value := DM1.qryNFCE_MID_CLIENTE.Value;
        DM1.qryConsulta.Open;


        if Length(TiraPontos(DM1.qryNFCE_MCPF_NOTA.AsString)) >= 11 then
        begin
          Dest.CNPJCPF := TiraPontos(DM1.qryNFCE_MCPF_NOTA.AsString);
          Dest.xNome := lblConsumidor.Caption;//DM1.qryConsulta.FieldByName('NOME').AsString;
          Dest.IE := ''; // Deixe a Inscrição Estadual vazia para CPF
          Dest.indIEDest := inNaoContribuinte; // Certifique-se de que está como não contribuinte
        end
        else
        BEGIN
          Dest.CNPJCPF := '';
          Dest.xNome := 'CONSUMIDOR FINAL';
          Dest.IE := '';
          Dest.indIEDest := inNaoContribuinte;
        END;
      end
      else
      begin
        if Length(TiraPontos(DM1.qryNFCE_MCPF_NOTA.AsString)) >= 9 then
          Dest.CNPJCPF := TiraPontos(DM1.qryNFCE_MCPF_NOTA.AsString)
        else
          if Length(TiraPontos(qryVendasCPF_NOTA.Value)) >= 9 then
            Dest.CNPJCPF := TiraPontos(qryVendasCPF_NOTA.Value)
          else
            Dest.CNPJCPF := '';
        Dest.xNome := 'CONSUMIDOR FINAL';
      end;

      // Endereço de Entrega (para NFC-e não é obrigatório)
      Entrega.xLgr := '';
      Entrega.nro := '';
      Entrega.xCpl := '';
      Entrega.xBairro := '';
      Entrega.xMun := '';
      Entrega.UF := '';

      // Itens da venda
      NumItem := 0;
      DM1.qryNFCE_D.First;
      while not DM1.qryNFCE_D.Eof do
      begin
        NumItem := NumItem + 1;
        with Det.Add do
        begin
          CodigoGTIN := ''; //Trim(OnlyNumber(DM1.QRYNFCE_DCOD_BARRA.AsString));
          if CodigoGTIN <> '' then
          begin
            DM1.ACBrValidador1.TipoDocto := docGTIN;
            DM1.ACBrValidador1.Documento := CodigoGTIN;
            if not DM1.ACBrValidador1.Validar then
              CodigoGTIN := '';
          end
          else
            CodigoGTIN := '';

          Prod.nItem   := NumItem;
          Prod.cProd   := DM1.qryNFCE_DID_PRODUTO.AsString;
          Prod.cEAN    := CodigoGTIN;
          Prod.xProd   := DM1.qryNFCE_DDESCRICAO.AsString;
          Prod.NCM     := DM1.qryNFCE_DNCM.AsString;
          Prod.CFOP    := DM1.qryNFCE_DCFOP.AsString;
          Prod.uCom    := DM1.QRYNFCE_DUNIDADE.AsString;
          Prod.uTrib   := DM1.QRYNFCE_DUNIDADE.AsString;
          Prod.qCom    := RoundABNT(DM1.QRYNFCE_DQTD.AsFloat, -4); // Quantidade com precisão maior
          Prod.qTrib   := RoundABNT(DM1.QRYNFCE_DQTD.AsFloat, -4); // Quantidade com precisão maior
          Prod.vUnCom  := RoundABNT(DM1.QRYNFCE_DPRECO.AsFloat, -2); // Preço unitário arredondado
          Prod.vUnTrib := RoundABNT(DM1.QRYNFCE_DPRECO.AsFloat, -2); // Preço unitário arredondado
          Prod.vProd   := RoundABNT(DM1.QRYNFCE_DVALOR_ITEM.AsCurrency, -2); // Valor total do produto  //RoundABNT(Prod.qCom * Prod.vUnCom, -2); // Valor total do produto

          // Adicione desconto e outros valores
          Prod.vDesc  := RoundABNT(DM1.QRYNFCE_DVDESCONTO.AsFloat, -2);
          Prod.vOutro := RoundABNT(DM1.qryNFCE_DOUTROS.AsFloat, -2);

          // ICMS
          Imposto.ICMS.orig := StrToOrig(vOK, Copy(DM1.QRYNFCE_DCST.Value, 1, 1));
          if not vOK then
            raise Exception.CreateFmt('Código origem "%d" inválida para o item "%s - %s"', [Copy(DM1.QRYNFCE_DCST.Value, 1, 1), DM1.qryNFCE_DID_PRODUTO.AsString, DM1.qryNFCE_DDESCRICAO.AsString]);

          if trim(GEmitente.Regime) = 'SIMPLES NACIONAL' then
          begin
            Imposto.ICMS.CSOSN := StrToCSOSNIcms(vOK, DM1.QRYNFCE_DCSOSN.AsString);
            if not vOK then
              raise Exception.CreateFmt('Código do CSOSN "%s" inválido para o item "%s - %s"', [DM1.QRYNFCE_DCSOSN.AsString, DM1.qryNFCE_DID_PRODUTO.AsString, DM1.qryNFCE_DDESCRICAO.AsString]);
          end
          else
          begin
            Imposto.ICMS.CST := StrToCSTICMS(vOK, Copy(DM1.QRYNFCE_DCST.AsString, 2, 2));
            if not vOK then
              raise Exception.CreateFmt('Código do CST "%s" inválido para o item "%s - %s"', [Copy(DM1.QRYNFCE_DCST.AsString, 2, 2), DM1.qryNFCE_DID_PRODUTO.AsString, DM1.qryNFCE_DDESCRICAO.AsString]);

            Imposto.ICMS.pICMS := DM1.QRYNFCE_DALIQ_ICMS.AsFloat;
          end;

          // PIS
          Imposto.PIS.CST := StrToCSTPIS(vOK, DM1.QRYNFCE_DCST_PIS.AsString);
          if not vOK then
            raise Exception.CreateFmt('Código CST do Pis "%d" inválido para o item "%s - %s"', [DM1.QRYNFCE_DCST_PIS.AsString, DM1.qryNFCE_DID_PRODUTO.AsString, DM1.qryNFCE_DDESCRICAO.AsString]);

          Imposto.PIS.vBC := DM1.QRYNFCE_DBASE_PIS_ICMS.AsFloat;
          Imposto.PIS.pPIS := DM1.QRYNFCE_DALIQ_PIS_ICMS.AsFloat / 100;

          // COFINS
          Imposto.COFINS.CST := StrToCSTCOFINS(vOK, DM1.QRYNFCE_DCST_COFINS.AsString);
          if not vOK then
            raise Exception.CreateFmt('Código CST do Cofins "%d" inválido para o item "%s - %s"', [DM1.QRYNFCE_DCST_COFINS.AsString, DM1.qryNFCE_DID_PRODUTO.AsString, DM1.qryNFCE_DDESCRICAO.AsString]);

          Imposto.COFINS.vBC := DM1.QRYNFCE_DBASE_COFINS_ICMS.AsFloat;
          Imposto.COFINS.pCOFINS := DM1.QRYNFCE_DALIQ_COFINS_ICMS.AsFloat / 100;

          // Imposto aproximado
          ////Imposto.vTotTrib := DM1.QRYNFCE_DTRIB_FED.AsFloat + DM1.QRYNFCE_DTRIB_EST.AsFloat + DM1.QRYNFCE_DTRIB_MUN.AsFloat;
         // Imposto.vTotTrib := RoundABNT(DM1.QRYNFCE_DTRIB_FED.AsFloat + DM1.QRYNFCE_DTRIB_EST.AsFloat + DM1.QRYNFCE_DTRIB_MUN.AsFloat, -2);
        end;

        DM1.qryNFCE_D.Next;
      end;


      // #Totais nota fiscal
      Total.ICMSTot.vBC     := DM1.qryNFCE_MBASEICMS.AsFloat;
      Total.ICMSTot.vICMS   := DM1.qryNFCE_MTOTALICMS.AsFloat;
      Total.ICMSTot.vProd   := DM1.qryNFCE_MSUBTOTAL.AsFloat;
      Total.ICMSTot.vDesc   := DM1.qryNFCE_MDESCONTO.AsFloat;
      Total.ICMSTot.vPIS    := DM1.qryNFCE_MTOTALICMSPIS.AsFloat;
      Total.ICMSTot.vCOFINS := DM1.qryNFCE_MTOTALICMSCOFINS.AsFloat;
      Total.ICMSTot.vOutro  := DM1.qryNFCE_MOUTROS.AsFloat;
      Total.ICMSTot.vNF     := DM1.qryNFCE_MTOTAL.AsFloat;

      // lei da transparencia de impostos
      if Parametro.VALOR_APROX_NFE then
        Total.ICMSTot.vTotTrib := Total.ICMSTot.vICMS+Total.ICMSTot.vPIS+Total.ICMSTot.vCOFINS //DM1.qryNFCE_MTRIB_MUN.AsFloat + DM1.qryNFCE_MTRIB_EST.AsFloat + DM1.qryNFCE_MTRIB_FED.AsFloat + DM1.qryNFCE_MTRIB_IMP.AsFloat
      else
        Total.ICMSTot.vTotTrib := 0;

      Transp.modFrete := mfSemFrete;

       // Formas de pagamento
      xobs := '';

      if ComboBox1.ItemIndex=0 then
      begin
        DM1.cdsPagto.First;

        while not DM1.cdsPagto.Eof do
        begin
          if DM1.cdsPagtoVALOR.Value > 0 then
          begin
            with pag.Add do
            begin
              // Mapeia a forma de pagamento conforme o tipo registrado
              if UpperCase(DM1.cdsPagtoTIPO.Value) = 'D' then
                tPag := fpDinheiro
              else if UpperCase(DM1.cdsPagtoTIPO.Value) = 'Q' then
                tPag := fpCheque
              else if UpperCase(DM1.cdsPagtoTIPO.Value) = 'C' then
                tPag := fpCartaoCredito
              else if UpperCase(DM1.cdsPagtoTIPO.Value) = 'E' then
                tPag := fpCartaoDebito
              else if UpperCase(DM1.cdsPagtoTIPO.Value) = 'L' then
                tPag := fpCreditoLoja
              else
              begin
                tPag := fpOutro; // Caso não identificado, define como "Outros"
                xPag := UpperCase(acento( DM1.cdsPagtoDESCRICAO.AsString)); // Descrição obrigatória para "Outros"
               end;

              // Define o valor do pagamento
              vPag := RoundABNT(DM1.cdsPagtoVALOR.AsFloat, -2);

              // Exemplo: Ajuste o campo adicional para o código da bandeira do cartão (se necessário)
              if (tPag = fpCartaoCredito) or (tPag = fpCartaoDebito) then
              begin
                cAut := '123456'; // Código de autorização (se houver)
                tBand := bcVisa;  // Exemplo de bandeira de cartão (ajuste conforme necessário)
              end;

              if UpperCase(acento( DM1.cdsPagtoDESCRICAO.AsString))='PIX' then
              begin
                 xobs := xobs +' PAG. EM PIX:  '+FormatFloat('0.00', DM1.cdsPagtoVALOR.AsFloat);
              end;
            end;
          end;

          DM1.cdsPagto.Next;
        end;
      end
      else
      begin
        with pag.Add do
        begin
          tPag := fpCreditoLoja;
          vPag := RoundABNT(StrToFloat(label51.caption), -2);
        end;
      end;


      xobs := xobs+'| Cliente: '+lblConsumidor.caption+' CPF/CNPJ: '+lb_CpfCnpj.Caption;

      // Define o troco, se aplicável
      if rtroco > 0 then
        pag.vTroco := rtroco;

      // Informações Adicionais
      infAdic.infCpl :=xObs+#13#10+' * Obrigado pela preferência! * ';

      try
        dm1.ACBrNFe1.NotasFiscais.GerarNFe;
//        dm1.ACBrNFe1.NotasFiscais.Validar;
        dm1.ACBrNFe1.NotasFiscais.Assinar;
//        dm1.ACBrNFe1.NotasFiscais.GravarXML('c:\teste\teste.xml');
        if DM1.ACBrNFe1.Enviar('1', false, True) then // passo 6 envia nfce
        begin
          sTransmitida;
        end;
      except
        on e: Exception do
        begin
          case DM1.ACBrNFe1.WebServices.Enviar.cStat of
            101: // cancelada
              sCancelada;
            110: // Nota denegada
              sDenegada;

            204, 539:
              begin // duplicidade
                DM1.ACBrNFe1.Consultar;

                if DM1.ACBrNFe1.WebServices.Consulta.cStat = 100 then
                  sTransmitida
                else
                  sDuplicidade;

              end;
          else
            begin
              case DM1.ACBrNFe1.WebServices.Retorno.cStat of
                101:
                  // cancelada
                  sCancelada;
                204, 539:
                  begin // duplicidade
                    DM1.ACBrNFe1.Consultar;
                    if DM1.ACBrNFe1.WebServices.Consulta.cStat = 100 then
                      sTransmitida
                    else
                      sDuplicidade;

                  end;
              else
                if (pos('DUPLICIDADE', e.Message.ToUpper) > 0) then
                begin
                  DM1.ACBrNFe1.Consultar;
                  if DM1.ACBrNFe1.WebServices.Consulta.cStat = 100 then
                    sTransmitida
                  else
                    sDuplicidade;
                end
                else
                begin
                  if (pos('12007 - ', e.Message.ToUpper) > 0) or
                    (pos('12002 - ', e.Message.ToUpper) > 0) or
                    (pos('12029 - ', e.Message.ToUpper) > 0) or
                    (pos('10060 - ', e.Message.ToUpper) > 0) or
                    (pos('TIMEOUT ', e.Message.ToUpper) > 0) or
                    (pos('TIME OUT ', e.Message.ToUpper) > 0) then
                    ShowMessage('Falha na conexão com o servidor!')
                  else
                    ShowMessage(e.Message);

                  exit;
                end;
              end;
            end;
          end;
        end;
      end;

      if Parametro.VALE_PRESENTE_BALCAO then
        valepresente;
    end;
  except
    on e: Exception do
      raise Exception.Create(e.Message);
  end;
end;

procedure TFbalcao.EnviarCFe(const ASerie, ANumero: Integer);
begin
  try
    DM1.AcbrSAT1.EnviarDadosVenda;
  except
    on e: Exception do
    begin
      raise Exception.CreateFmt('Ocorreu o seguinte erro ao tentar enviar o CF-e:' + sLineBreak + e.Message + sLineBreak + sLineBreak + '%d - %s', [DM1.AcbrSAT1.Resposta.codigoDeErro, DM1.AcbrSAT1.Resposta.mensagemRetorno]);
    end;
  end;

  if DM1.AcbrSAT1.Resposta.codigoDeRetorno = 6000 then
  begin
    try
      DM1.ACBrSATExtratoFortes1.LarguraBobina := Parametro.LarguraBobina;
      DM1.ACBrSATExtratoFortes1.MARGEMDIREITA := Parametro.MARGEMDIREITA;
      DM1.ACBrSATExtratoFortes1.MARGEMESQUERDA := Parametro.MARGEMESQUERDA;

      if Parametro.IMPRIMIR_CUPOM then
        DM1.AcbrSAT1.ImprimirExtrato
      else
        iF MessageDlg('DESEJA IMPRIMIR O CUPOM?', mtconfirmation, [mbYes, mbNo], 0) = mrYes then
          DM1.AcbrSAT1.ImprimirExtrato;

    except
      on e: Exception do
        raise Exception.Create(e.Message);
    end;
    GravarCFe(ASerie, ANumero, DM1.AcbrSAT1.CFe.ide.nserieSAT, DM1.AcbrSAT1.CFe.ide.nCFe, OnlyNumber(DM1.AcbrSAT1.CFe.infCFe.id), '', DM1.AcbrSAT1.CFe.ide.dEmi + DM1.AcbrSAT1.CFe.ide.hEmi, DM1.AcbrSAT1.CFe.AsXMLString);

    if Parametro.VALE_PRESENTE_BALCAO then
       valepresente;
  end
  else
    raise Exception.CreateFmt('%d - %s', [DM1.AcbrSAT1.Resposta.codigoDeErro, DM1.AcbrSAT1.Resposta.mensagemRetorno]);
end;

function TFbalcao.GravarCFe(const ASerie, ANumero, ASerieSat, ANumeroSat: Integer; const AChave, ANumeroProtocolo: String; const ADataHoraRecto: TDateTime; const AXML: String): Boolean;
begin
  if not(DM1.qryNFCE_M.State in dsEditModes) then
    DM1.qryNFCE_M.Edit;
  DM1.qryNFCE_MPROTOCOLO.Value := ANumeroProtocolo;
  DM1.qryNFCE_MXML.Value := AXML;
  DM1.qryNFCE_MCHAVE.Value := AChave;
  DM1.qryNFCE_MSAT_NUMERO_CFE.Value := ANumeroSat;
  DM1.qryNFCE_MSAT_NUMERO_SERIE.AsInteger := ASerieSat;
  DM1.qryNFCE_MFLAG.Value := 'N';
  DM1.qryNFCE_MABERTO.Value := 'N';
  DM1.qryNFCE_MSITUACAO.Value := 'T';
  DM1.qryNFCE_MDATA_EMISSAO.Value := ADataHoraRecto;
  DM1.qryNFCE_MDATA_SAIDA.Value := ADataHoraRecto;
  DM1.qryNFCE_MHORA_EMISSAO.Value := ADataHoraRecto;
  DM1.qryNFCE_MHORA_SAIDA.Value := ADataHoraRecto;
  DM1.qryNFCE_MFK_VENDA.Value := StrToInt(StaticText2.Caption);
  DM1.qryNFCE_M.Post;
  if DM1.qryNFCE_M.Transaction.Active then
    DM1.qryNFCE_M.Transaction.Commit;
end;

procedure TFbalcao.imgImpClick(Sender: TObject);
var
  InputString: string;
begin
  PostMessage(Handle, InputBoxMessage, 0, 0);
  InputString := InputBox('Senha', 'Digite a senha', '');
  if InputString = '159753' then
  begin
    if fimp = nil then
      fimp := Tfimp.Create(Application);

    RLPrinter.PrinterName := Parametro.impressora;

    fimp.RLReport10.PrintDialog := false;

    fimp.RLReport10.Print;

    fimp.Release;
    fimp := nil;
    Fbalcao.SetFocus;
  end
  else
  begin
    ShowMessage('Senha inválida');
  end;
end;

procedure TFbalcao.ImportaPedido;
var
  I: Integer;
begin
  try
    // # Solicita se o cliente quer algum documento no cupom
    SolicitaCPF_CNPJ_CUPOM;

//    SolicitaNOME_RAZAO_CUPOM;

    qryItem.DisableControls;
    qryItem.Close;
    qryItem.Params[0].AsString := StaticText2.Caption;
    qryItem.open;

    // # Importa cabecalho do pedido
    DM1.qryNFCE_M.Close;
    DM1.qryNFCE_M.Params[0].Value := StrToInt(StaticText2.Caption);
    DM1.qryNFCE_M.open;
    if DM1.qryNFCE_M.IsEmpty then
    begin
      DM1.qryNFCE_M.Insert;
      DM1.qryNFCE_MSERIE.Value := DM1.qryTerminalSERIE.Value;
      DM1.qryNFCE_MCODIGO.Value := DM1.Numerador('NFCE_MASTER', 'CODIGO', 'N', '', '');
      DM1.qryExecute.Close;

      DM1.qryExecute.SQL.Text := 'SELECT COALESCE(MAX(NUMERO),0) qtd FROM NFCE_MASTER WHERE SERIE=:SERIE AND FKEMPRESA=:EMPRESA';
      DM1.qryExecute.Params[0].Value := DM1.qryTerminalSERIE.Value;
      DM1.qryExecute.Params[1].Value := GEmitente.IDEmitente;
      DM1.qryExecute.open;

      if DM1.qryExecute.Fields[0].AsInteger = 0 then
        DM1.qryNFCE_MNUMERO.Value := DM1.qryTerminalNUMERACAO_INICIAL.Value
      else
        DM1.qryNFCE_MNUMERO.Value := DM1.qryExecute.Fields[0].AsInteger + 1;
      DM1.qryNFCE_MCNF.AsInteger := StrToInt(StaticText2.Caption);

       dm1.qryNFCE_MMODELO.Value := '65';
      if TipoDocAtual=tdCFe then
        DM1.qryNFCE_MMODELO.Value := '59';

      DM1.qryNFCE_MSITUACAO.Value := 'G';
      DM1.qryNFCE_MFK_VENDA.Value := qryVendasNOTA.AsInteger;
    end
    else
      DM1.qryNFCE_M.Edit;
    DM1.qryNFCE_MFKEMPRESA.Value := GEmitente.IDEmitente;
    DM1.qryNFCE_MDATA_EMISSAO.Value := date;
    DM1.qryNFCE_MDATA_SAIDA.Value := date;
    DM1.qryNFCE_MDINHEIRO.AsFloat := RoundTo(qryVendasDINHEIRO.AsFloat - qryVendasDESCONTO.AsFloat, -2);
    DM1.qryNFCE_MOBSERVACOES.Value := qryVendasOBS.Value;
    if DM1.qryNFCE_MID_CLIENTE.IsNull then
      DM1.qryNFCE_MID_CLIENTE.Value := qryVendasCLIENTE.Value;
    DM1.qryNFCE_MFK_USUARIO.Value := qryVendasOPERADOR.Value;
    DM1.qryNFCE_MFK_CAIXA.Value := StrToInt(DM1.TERMINAL);
    DM1.qryNFCE_MSERIE.Value := DM1.qryTerminalSERIE.Value;
    DM1.qryNFCE_MFK_VENDEDOR.Value := qryVendasVENDEDOR.Value;
    DM1.qryNFCE_MCPF_NOTA.Value := FCPF_CNPJ; // qryVendasCPF_NOTA.Value;
//    DM1.qryNFCE_MNOME_NOTA.Value := FNOME_RAZAO;
    DM1.qryNFCE_MSUBTOTAL.AsFloat := qryVendasTOTAL.AsFloat + qryVendasDESCONTO.AsFloat - qryVendasACRESCIMO.AsFloat;;
    DM1.qryNFCE_MOUTROS.AsFloat := qryVendasACRESCIMO.AsFloat;
    DM1.qryNFCE_MDESCONTO.AsFloat := qryVendasDESCONTO.AsFloat;
    DM1.qryNFCE_MTOTAL.AsFloat := qryVendasTOTAL.AsFloat; // - qryVendasDESCONTO.AsFloat + qryVendasACRESCIMO.AsFloat;
    // dm1.qryNFCE_MTIPO_DESCONTO.Value := qryVendasTIPO_DESCONTO.Value;
    DM1.qryNFCE_MTROCO.AsFloat := qryVendasTROCO.AsFloat;
    DM1.qryNFCE_M.Post;
    if DM1.qryNFCE_M.Transaction.Active then
      DM1.qryNFCE_M.Transaction.Commit;

    DM1.qryNFCE_M.Close;
    DM1.qryNFCE_M.Params[0].Value := StrToInt(StaticText2.Caption);
    DM1.qryNFCE_M.open;

    DM1.qryExecute.Close;
    DM1.qryExecute.SQL.Text := 'delete from nfce_detalhe where fkvenda=:fk';
    DM1.qryExecute.Params[0].Value := DM1.qryNFCE_MCODIGO.Value;
    DM1.qryExecute.ExecSQL;

    // # Itens
    DM1.qryNFCE_D.Close;
    DM1.qryNFCE_D.Params[0].Value := DM1.qryNFCE_MCODIGO.Value;
    DM1.qryNFCE_D.open;

    I := 1;
    qryItem.First;
    while not qryItem.Eof do
    begin
      qryProd.Close;
      qryProd.ParamByName('CODIGO_PRODUTO').AsString := qryItemCODIGO.AsString;
      qryProd.open;

      qryConsulta.Close;
      qryConsulta.SQL.Text := 'SELECT GEN_ID(NFCE_DETALHE_GEN, 1) AS CODIGO FROM RDB$DATABASE';
      qryConsulta.open;

      DM1.qryNFCE_D.Insert;
      //DM1.QRYNFCE_DCODIGO.Value := DM1.Numerador('NFCE_DETALHE', 'CODIGO', 'N', '', '');
      DM1.QRYNFCE_DCODIGO.Value := qryconsulta.FieldByName('codigo').AsInteger;  //DM1.Numerador('NFCE_DETALHE', 'CODIGO', 'N', '', '');
      dm1.qryconsulta.close;

      DM1.QRYNFCE_DFKVENDA.Value := DM1.qryNFCE_MCODIGO.Value;
      DM1.qryNFCE_DID_PRODUTO.Value := qryItemCODIGO.Value;
      DM1.qryNFCE_DITEM.Value := I;
      DM1.QRYNFCE_DCOD_BARRA.Value := qryProdCODIGOBARRA.Value;
      DM1.qryNFCE_DNCM.Value := qryProdNCM.Value;
      DM1.qryNFCE_DCFOP.Value := qryProdCFOP.Value;
      DM1.QRYNFCE_DCST.Value := qryProdCST_ORIGEM.Value + qryProdCST.Value;
      DM1.QRYNFCE_DCST_PIS.Value := qryProdCST_PIS.Value;
      DM1.QRYNFCE_DCST_COFINS.Value := qryProdCST_COFINS.Value;
      DM1.QRYNFCE_DCSOSN.Value := qryProdCSOSN.Value;
      DM1.QRYNFCE_DCEST.Value := qryProdCEST.Value;
      // dm1.QRYNFCE_DTIPO.Value := qryProd.Value;
      DM1.QRYNFCE_DQTD.Value := qryItemQTD.AsFloat;
      DM1.QRYNFCE_DPRECO.AsFloat := qryItemPRECO.AsFloat;
      DM1.QRYNFCE_DVALOR_ITEM.AsFloat := qryItemTOTAL.AsFloat;
      DM1.QRYNFCE_DTOTAL.AsFloat := qryItemTOTAL.AsFloat;
      DM1.QRYNFCE_DUNIDADE.Value := qryItemUND.Value;
      DM1.QRYNFCE_DVDESCONTO.AsFloat := qryItemDESCONTO.AsFloat + qryItemDESCONTO1.AsFloat ;
      DM1.qryNFCE_DOUTROS.AsFloat := qryItemACRESCIMO.AsFloat;
      DM1.QRYNFCE_DALIQ_ICMS.AsFloat := qryProdICMS.Value;
      DM1.qryNFCE_DBASE_ICMS.AsFloat := 0;
      if DM1.QRYNFCE_DALIQ_ICMS.AsFloat > 0 then
        DM1.qryNFCE_DBASE_ICMS.AsFloat := qryItemTOTAL.AsFloat;
      DM1.qryNFCE_DVALOR_ICMS.AsFloat := SimpleRoundTo((qryProdICMS.AsFloat * qryItemTOTAL.AsFloat) / 100, -2);

      DM1.QRYNFCE_DALIQ_COFINS_ICMS.AsFloat := qryProdALIQ_COFINS.Value;
      DM1.QRYNFCE_DBASE_COFINS_ICMS.AsFloat := 0;
      if DM1.QRYNFCE_DALIQ_COFINS_ICMS.AsFloat > 0 then
        DM1.QRYNFCE_DBASE_COFINS_ICMS.AsFloat := qryItemTOTAL.AsFloat;
      DM1.qryNFCE_DVALOR_COFINS_ICMS.AsFloat := SimpleRoundTo((qryProdALIQ_COFINS.AsFloat * qryItemTOTAL.AsFloat / 100), -2);

      DM1.QRYNFCE_DALIQ_PIS_ICMS.Value := qryProdALIQ_PIS.Value;
      DM1.QRYNFCE_DBASE_PIS_ICMS.Value := 0;
      if DM1.QRYNFCE_DALIQ_PIS_ICMS.Value > 0 then
        DM1.QRYNFCE_DBASE_PIS_ICMS.Value := qryItemTOTAL.AsFloat;
      DM1.qryNFCE_DVALOR_PIS_ICMS.Value := SimpleRoundTo((qryProdALIQ_PIS.Value * qryItemTOTAL.AsFloat / 100), -2);

      DM1.QRYNFCE_DBASE_ISS.Value := 0;
      DM1.QRYNFCE_DALIQ_ISS.Value := 0;
      DM1.QRYNFCE_DVALOR_ISS.Value := 0;

      qryIBPT.Close;
      qryIBPT.Params[0].Value := qryProdNCM.Value;
      qryIBPT.open;
      if qryIBPT.IsEmpty then
      begin
        ShowMessage('NCM do produto ' + qryProdID.AsString + '-' + qryProdDESCRICAO.AsString + #13 + 'Não foi encontrado!');
        exit;
      end;

      DM1.QRYNFCE_DTRIB_MUN.AsFloat := SimpleRoundTo(qryIBPTMUNICIPAL.Value * qryItemTOTAL.AsFloat / 100, -2);
      DM1.QRYNFCE_DTRIB_EST.AsFloat := SimpleRoundTo(qryIBPTESTADUAL.Value * qryItemTOTAL.AsFloat / 100, -2);
      DM1.QRYNFCE_DTRIB_FED.AsFloat := SimpleRoundTo(qryIBPTNACIONALFEDERAL.Value * qryItemTOTAL.AsFloat / 100, -2);
      DM1.QRYNFCE_DTRIB_IMP.AsFloat := SimpleRoundTo(qryIBPTIMPORTADOSFEDERAL.Value * qryItemTOTAL.AsFloat / 100, -2);
      DM1.qryNFCE_D.Post;
      Inc(I);
      qryItem.Next;
    end;

    // Atualiza os totais do cabeçalho da nota
    qrySomaNFCe.Close;
    qrySomaNFCe.Params[0].Value := DM1.qryNFCE_MCODIGO.Value;
    qrySomaNFCe.open;

    DM1.qryNFCE_M.Edit;
    DM1.qryNFCE_MBASEICMS.Value := SimpleRoundTo(qrySomaNFCeBASE_ICMS.AsFloat, -2);
    DM1.qryNFCE_MTOTALICMS.Value := SimpleRoundTo(qrySomaNFCeVALOR_ICMS.AsFloat, -2);
    DM1.qryNFCE_MBASEISS.Value := SimpleRoundTo(qrySomaNFCeBASE_ISS.AsFloat, -2);
    DM1.qryNFCE_MTOTALISS.Value := SimpleRoundTo(qrySomaNFCeVALOR_ISS.AsFloat, -2);
    DM1.qryNFCE_MBASEICMSPIS.Value := SimpleRoundTo(qrySomaNFCeBASE_PIS_ICMS.AsFloat, -2);
    DM1.qryNFCE_MTOTALICMSPIS.Value := SimpleRoundTo(qrySomaNFCeVALOR_PIS_ICMS.AsFloat, -2);
    DM1.qryNFCE_MBASEICMSCOF.Value := SimpleRoundTo(qrySomaNFCeBASE_COF_ICMS.AsFloat, -2);
    DM1.qryNFCE_MTOTALICMSCOFINS.Value := SimpleRoundTo(qrySomaNFCeVALOR_COF_ICMS.AsFloat, -2);
    DM1.qryNFCE_MTRIB_MUN.Value := SimpleRoundTo(qrySomaNFCeTOTALMUN.AsFloat, -2);
    DM1.QRYNFCE_MTRIB_IMP.Value := SimpleRoundTo(qrySomaNFCeTOTALIMP.AsFloat, -2);
    DM1.qryNFCE_MTRIB_EST.Value := SimpleRoundTo(qrySomaNFCeTOTALEST.AsFloat, -2);
    DM1.qryNFCE_MTRIB_FED.Value := SimpleRoundTo(qrySomaNFCeTOTALFED.AsFloat, -2);
    DM1.qryNFCE_MSUBTOTAL.Value := SimpleRoundTo(qrySomaNFCeTOTAL.AsFloat + qrySomaNFCeDESCONTOS.AsFloat - qrySomaNFCeOUTROS.AsFloat, -2);
    DM1.qryNFCE_MDESCONTO.Value := SimpleRoundTo(qrySomaNFCeDESCONTOS.AsFloat, -2); //0 alterado 22/03
    DM1.qryNFCE_MOUTROS.Value := SimpleRoundTo(qrySomaNFCeOUTROS.AsFloat, -2); //0 incluido 22/03
    DM1.qryNFCE_MTOTAL.Value := qrySomaNFCeTOTAL.AsFloat; // + qrySomaNFCeOUTROS.AsFloat; alterado 22/03
    DM1.qryNFCE_MTROCO.Value := rTROCO; //inluido 22/03

    DM1.qryNFCE_M.Post;
    if DM1.qryNFCE_M.Transaction.Active then
      DM1.qryNFCE_M.Transaction.Commit;
    // dm1.TRANS2.Commit;
  finally
    qryItem.EnableControls;
  end;
end;

procedure TFbalcao.GravarRecebimento(ACodFormaPagto, AMoeda: Integer; AValor: Double);
var
  lCodFormPagto: Integer;
begin
  try
    lCodFormPagto := ACodFormaPagto;
    if (ACodFormaPagto = 0) then
    begin
      qryConsulta.Close;
      qryConsulta.SQL.Clear;
      qryConsulta.SQL.Text := 'SELECT ID FROM FORMA_PAGAMENTO WHERE MOEDA = :MOEDA';
      qryConsulta.ParamByName('MOEDA').AsInteger := AMoeda;
      qryConsulta.open;

      lCodFormPagto := qryConsulta.FieldByName('ID').AsInteger;
    end;

    qryTMPReceb.ParamByName('ID_FORMA_PAGAMENTO').AsInteger := lCodFormPagto;
    qryTMPReceb.ParamByName('NOTA').AsString := StaticText2.Caption;
    qryTMPReceb.ParamByName('N_CAIXA').AsInteger := StrToIntDef(DM1.TERMINAL, 0);
    qryTMPReceb.ParamByName('VALOR').AsFloat := AValor;
    qryTMPReceb.ParamByName('TROCO').AsFloat := 0;
    qryTMPReceb.ExecSQL;
  Except
    on e: Exception do
    begin
      if qryTMPReceb.Transaction.Active then
        qryTMPReceb.Transaction.Rollback;
      MessageDlg('Erro ao Gravar Recebimento' + #1013 + 'Erro:' + e.Message, mtError, [mbOK], 0);
    end;
  end;
end;

procedure TFbalcao.SelecionaMetodoPagto;
var
  Msg: string;
  lLimpaVenda: Boolean;
begin
  Especie := 'DINHEIRO';
  if listMetodoPagto.ItemIndex <> 0 then
    Especie := 'CARTEIRA';

  FMetodoPagto := listMetodoPagto.Items.Strings[listMetodoPagto.ItemIndex];

  { if Gravar_Venda(lLimpaVenda) then
    begin
    if cancela_desmembra = 1 then
    begin
    Atualiza_Valores_Tela;
    Exit;
    end
    else
    Msg := 'Pedido efetuado com Sucesso.';
    end
    else
    if lLimpaVenda then
    Msg := 'Venda não efetuada, favor contatar o suporte.';
  }

  if lLimpaVenda then
  begin
    MessageDlg(Msg, mtInformation, [mbOK], 0);
    limpa;
    limpa;
    Fbalcao.SetFocus;
    // Ctrltela(False);
    ShowHidePanel(pnlFormaPgto, false);
  end;
end;

procedure TFbalcao.selecionar_item;
begin
  qryProdItem.Close;
  qryProdItem.SQL.Clear;
  qryProdItem.SQL.add('select id , prod_id , qtd , valor from proditens where prod_id = :codigo ');
  qryProdItem.ParamByName('codigo').AsInteger := qryProduto.FieldByName('id').AsInteger;
  qryProdItem.open;

  if not qryProdItem.IsEmpty then
  begin
    frmselecionarItem := TfrmselecionarItem.Create(Application);
    try
      frmselecionarItem.sit := '1';
      frmselecionarItem.cod_produto := qryProduto.FieldByName('id').AsInteger;
      frmselecionarItem.showmodal;
    except
      FreeAndNil(frmselecionarItem);
    end;
  end;
end;

procedure TFbalcao.ShowHidePanel(Sender: TPanel; Isvisible: Boolean);
begin
  Sender.Visible := false;
  if Isvisible then
  begin
    Sender.Top := (pnlDesktop.Height div 2) - (Sender.Height div 2);
    Sender.Left := (pnlDesktop.Width div 2) - (Sender.Width div 2);
    Sender.Visible := Isvisible;
  end;
end;

procedure TFbalcao.SolicitaCPF_CNPJ_CUPOM;
begin
  if lb_CpfCnpj.Caption = '' then
  FCPF_CNPJ := ''
  else
  FCPF_CNPJ := lb_CpfCnpj.Caption;

  while true do
  begin
    if not InputQuery('CPF/CNPJ na nota ?', 'CPF/CNPJ na nota ? (Somente números)', FCPF_CNPJ) then
      break;

    if Trim(FCPF_CNPJ) <> '' then
    begin
      FCPF_CNPJ := OnlyCNPJorCPF(FCPF_CNPJ);
      if Trim(FCPF_CNPJ) <> '' then
        break
      else
        if not MsgInterrogacao('CPF/CNPJ inválido.', 'Gostaria de informar um documento válido?') then
          break;
    end
    else
      break;
  end;
end;

procedure TFbalcao.SolicitaNOME_RAZAO_CUPOM;
begin
  FNOME_RAZAO := '';
  while true do
  begin
    if not InputQuery('NOME/RAZÃO SOCIAL na nota ?', 'NOME/RAZÃO SOCIAL na nota ? (Sem Acentos e caracteres especiais)', FNOME_RAZAO) then
      break;

//    if Trim(FCPF_CNPJ) <> '' then
//    begin
//      FCPF_CNPJ := OnlyCNPJorCPF(FCPF_CNPJ);
//      if Trim(FCPF_CNPJ) <> '' then
//        break
//      else
//        if not MsgInterrogacao('CPF/CNPJ inválido.', 'Gostaria de informar um documento válido?') then
//          break;
//    end
//    else
//      break;
  end;
end;

procedure TFbalcao.AbreFormaPagamento;
begin
  ShowHidePanel(pnlFormaPgto, true);
  listMetodoPagto.ItemIndex := Parametro.formapagamento;
  if listMetodoPagto.CanFocus then
    listMetodoPagto.SetFocus;
end;

procedure TFbalcao.Limite;
var
  v1, v2, v3: real;
begin
  v1 := 0;
  v2 := 0;
  v3 := 0;

  if Label9.Caption <> '0' then
  begin
    qryLimite.Close;
    qryLimite.SQL.Text := 'select limite from clientes where id = :d0';
    qryLimite.ParamByName('d0').AsInteger := StrToInt(Label9.Caption);
    qryLimite.open;
    v1 := qryLimite.FieldByName('limite').AsFloat;
    qryLimite.Close;
    qryLimite.SQL.Text := 'select sum(valor)as valor from receber where codigo = :d0 and valor_recebido = 0 ';
    qryLimite.ParamByName('d0').AsInteger := StrToInt(Label9.Caption);
    qryLimite.open;
    v2 := qryLimite.FieldByName('valor').AsFloat;
    v3 := v2 - v1;
    if v3 > 0 then
    begin
      if Parametro.BLOQ_LIMITE then
      begin
        ChamaLimite;
        limite_ultrapassado := true;
      end;
    end;
  end
  else
  begin
    limite_ultrapassado := false;
  end;
end;

procedure TFbalcao.ChamaLimite;
begin
  FrmLimite := TFrmLimite.Create(Application);
  try
    FrmLimite.showmodal;
  finally
    FreeAndNil(FrmLimite);
  end;
end;

procedure TFbalcao.imprimir_np;
var
  valor: string[160];
  VALOR1: real;
  totalizador: Currency;
begin
  totalizador := 0;
  VALOR1 := 0;

  qryreceber.Close;
  qryreceber.SQL.Clear;
  qryreceber.SQL.Text := 'select * from receber where numero = :numero and emissao = :data ';
  qryreceber.ParamByName('numero').AsString := StaticText2.Caption;
  qryreceber.ParamByName('data').AsDate := StrToDate(Label11.Caption);
  qryreceber.open;

  // boleto - parte cima

  qryClientes.Close;
  qryClientes.SQL.Clear;
  qryClientes.SQL.add('select * from clientes where id = :codigo');
  qryClientes.ParamByName('codigo').AsString := AjustaStr_zero_esq(Label9.Caption, 5);
  qryClientes.open;

  if PARAMETRO.RAZAOSOCIAL <> '0' then
  ppLabel80.Caption := parametro.RAZAOSOCIAL else
  ppLabel80.Caption := GEmitente.RazaoSocial; // qryClientes.FieldByName('nome').AsString;

  // boleto - dados cliente
  ppLabel4.Caption := qryClientes.FieldByName('nome').AsString;
  ppLabel5.Caption := qryClientes.FieldByName('cpfcnpj').AsString;
  ppLabel6.Caption := qryClientes.FieldByName('rgie').AsString;
  ppLabel7.Caption := qryClientes.FieldByName('end1').AsString + ' ' + qryClientes.FieldByName('num1').AsString;
  ppLabel95.Caption := qryClientes.FieldByName('bairro1').AsString;
  ppLabel94.Caption := qryClientes.FieldByName('compl1').AsString;
  ppLabel98.Caption := qryClientes.FieldByName('cidade1').AsString;
  ppLabel99.Caption := qryClientes.FieldByName('uf1').AsString;
  ppLabel100.Caption := qryClientes.FieldByName('cep1').AsString;

  Boleto.ShowPrintDialog := false;

  Boleto.Print;
  // final boleto

  // inicio np
  ppLabel183.Caption := GEmitente.cidade + ' - ' + GEmitente.UF;
  ppLabel102.Caption := qryClientes.FieldByName('cidade1').AsString + ' - ' + qryClientes.FieldByName('uf1').AsString + ', ' + formatDateTime('dddddd', now);

  if PARAMETRO.RAZAOSOCIAL <> '0' then
  ppLabel103.Caption := Parametro.RAZAOSOCIAL else
  ppLabel103.Caption := GEmitente.RazaoSocial; // qryClientes.FieldByName('nome').AsString;

  ppLabel104.Caption := GEmitente.CPFCNPJ;

  ppLabel184.Caption := qryClientes.FieldByName('nome').AsString;
  ppLabel187.Caption := qryClientes.FieldByName('end1').AsString + ', ' + qryClientes.FieldByName('num1').AsString;
  ppLabel188.Caption := qryClientes.FieldByName('cpfcnpj').AsString + '          RG Nº: ' + qryClientes.FieldByName('rgie').AsString;

  totalizador := StrToCurr(Label16.Caption);
  valor := extenso(totalizador);
  ppLabel178.Caption := Copy(valor, 1, 79) + '-';

  if Copy(valor, 80, 84) = '     ' then
    ppLabel177.Caption := ('********************************************************************************')
  else
    ppLabel177.Caption := Copy(valor, 80, 160);

  Promissoria.ShowPrintDialog := false;

  Promissoria.Print;
end;

procedure TFbalcao.altera_atacado;
begin
  qryConsulta.Close;
  qryConsulta.SQL.Clear;
  qryConsulta.SQL.add(' select f.cupom, f.item, f.codigo , f.qtd, p.atacado, (f.qtd*p.atacado)as total from frente_tmpitvendas f,produtoeservicoempresa p where f.codigo = p.id and f.cupom = :nota and f.tipo = 1 and p.atacado <> 0');
  qryConsulta.ParamByName('nota').AsString := StaticText2.Caption;
  qryConsulta.open;
  qryConsulta.First;

  while not qryConsulta.Eof do
  begin
    qryExecutar.Close;
    qryExecutar.SQL.Clear;
    qryExecutar.SQL.add('update frente_tmpitvendas set preco = :preco , total = :total where cupom = :nota and item = :item and tipo = 1 ');
    qryExecutar.ParamByName('preco').AsFloat := qryConsulta.FieldByName('atacado').AsFloat;
    qryExecutar.ParamByName('total').AsFloat := qryConsulta.FieldByName('total').AsFloat;
    qryExecutar.ParamByName('nota').AsString := StaticText2.Caption;
    qryExecutar.ParamByName('item').AsInteger := qryConsulta.FieldByName('item').AsInteger;
    qryExecutar.ExecSQL;

    qryConsulta.Next;
  end;
  atualiza;
  MessageDlg('Valores dos produtos atualizados com valor de ATACADO.', mtInformation, [mbOK], 0);
end;

procedure TFbalcao.altera_varejo;
begin
  qryConsulta.Close;
  qryConsulta.SQL.Clear;
  qryConsulta.SQL.add(' select f.cupom, f.item, f.codigo , f.qtd, p.precovenda, (f.qtd*p.precovenda)as total from frente_tmpitvendas f,produtoeservicoempresa p where f.codigo = p.id and f.cupom = :nota and f.tipo = 1 and p.atacado <> 0');
  qryConsulta.ParamByName('nota').AsString := StaticText2.Caption;
  qryConsulta.open;
  qryConsulta.First;

  while not qryConsulta.Eof do
  begin
    qryExecutar.Close;
    qryExecutar.SQL.Clear;
    qryExecutar.SQL.add('update frente_tmpitvendas set preco = :preco , total = :total where cupom = :nota and item = :item and tipo = 1');
    qryExecutar.ParamByName('preco').AsFloat := qryConsulta.FieldByName('precovenda').AsFloat;
    qryExecutar.ParamByName('total').AsFloat := qryConsulta.FieldByName('total').AsFloat;
    qryExecutar.ParamByName('nota').AsString := StaticText2.Caption;
    qryExecutar.ParamByName('item').AsInteger := qryConsulta.FieldByName('item').AsInteger;
    qryExecutar.ExecSQL;

    qryConsulta.Next;
  end;
  atualiza;
  MessageDlg('Valores dos produtos atualizados com valor de VAREJO.', mtInformation, [mbOK], 0);
end;

function TFbalcao.StrIsFloat(const S: string): Boolean;
begin
  try
    StrToFloat(S);
    result := true;
  except
    result := false;
  end;
end;

procedure TFbalcao.Imprimir_40_New_01;
var
  cidade, sigla: string[40];
  compl, cep: string;
  tel, ende, bairro, num, Fantasia: string;
  codemitente: Integer;
  PDF, PASTA: string;
  datapdf: string;
begin
  if not Parametro.PRODUTO then
  begin
    if Label9.Caption <> '0' then
    begin
      DM1.q2.Close;
      DM1.q2.SQL.Text := 'SELECT * FROM PESSOA WHERE ID = :D0';
      DM1.q2.ParamByName('d0').AsInteger := StrToInt(Label9.Caption);
      DM1.q2.open;
      DM1.q3.Close;
      DM1.q3.SQL.Text := 'SELECT * FROM ENDERECO WHERE PESSOAID = :D0';
      DM1.q3.ParamByName('d0').AsInteger := DM1.q2.FieldByName('ID').AsInteger;
      DM1.q3.open;
      DM1.q4.Close;
      DM1.q4.SQL.Text := 'SELECT * FROM MUNICIPIO WHERE ID = :D0';
      DM1.q4.ParamByName('d0').AsInteger := DM1.q3.FieldByName('MUNICIPIOID').AsInteger;
      DM1.q4.open;
      DM1.Q5.Close;
      DM1.Q5.SQL.Text := 'SELECT * FROM UF WHERE ID = :D0';
      DM1.Q5.ParamByName('d0').AsInteger := DM1.q4.FieldByName('UFID').AsInteger;
      DM1.Q5.open;
      DM1.Q6.Close;
      DM1.Q6.SQL.Text := 'SELECT * FROM TELEFONE WHERE PESSOAID = :D0';
      DM1.Q6.ParamByName('d0').AsInteger := StrToInt(Label9.Caption);
      DM1.Q6.open;
      tel := DM1.Q6.FieldByName('NUMERO').AsString;
      ende := DM1.q3.FieldByName('LOGRADOURO').AsString;
      num := DM1.q3.FieldByName('NUMERO').AsString;
      bairro := DM1.q3.FieldByName('bairro').AsString;
      cidade := DM1.q4.FieldByName('NOME').AsString;
      sigla := DM1.Q5.FieldByName('SIGLA').AsString;
    end;
  end
  else
  begin
    if Label9.Caption <> '0' then
    begin
      qryConsulta.Close;
      qryConsulta.SQL.Clear;
      qryConsulta.SQL.add('SELECT * FROM CLIENTES WHERE ID = :D0');
      qryConsulta.ParamByName('d0').AsInteger := StrToInt(Label9.Caption);
      qryConsulta.open;
      tel := qryConsulta.FieldByName('telefone').AsString;
      if not Parametro.IMP_ENDERECO then
      begin
        ende := qryConsulta.FieldByName('end1').AsString;
        num := qryConsulta.FieldByName('num1').AsString;
        bairro := qryConsulta.FieldByName('bairro1').AsString;
        cidade := qryConsulta.FieldByName('cidade1').AsString;
        sigla := qryConsulta.FieldByName('uf1').AsString;
        compl := qryConsulta.FieldByName('compl1').AsString;
        cep := qryConsulta.FieldByName('cep1').AsString;
      end
      else
      begin
        ende := qryConsulta.FieldByName('end2').AsString;
        num := qryConsulta.FieldByName('num2').AsString;
        bairro := qryConsulta.FieldByName('bairro2').AsString;
        cidade := qryConsulta.FieldByName('cidade2').AsString;
        sigla := qryConsulta.FieldByName('uf2').AsString;
        compl := qryConsulta.FieldByName('compl2').AsString;
        cep := qryConsulta.FieldByName('cep2').AsString;
      end;
    end;
  end;
  if fimp = nil then
    fimp := Tfimp.Create(Application);
  fimp.RLReport3.DataSource := DSTmpItens;
  fimp.RLDBText22.DataSource := DSTmpItens;
  fimp.RLDBText23.DataSource := DSTmpItens;
  fimp.RLDBText24.DataSource := DSTmpItens;
  fimp.RLDBText25.DataSource := DSTmpItens;
  fimp.RLDBText26.DataSource := DSTmpItens;
  fimp.RLDBText27.DataSource := DSTmpItens;
  fimp.RLDBText28.DataSource := DSTmpItens;
  fimp.RLDBText63.DataSource := DSTmpItens;
  fimp.RLDBText63.DataSource := DSTmpItens;
  fimp.RLDBText64.DataSource := DSTmpItens;
  fimp.RLDBText65.DataSource := DSTmpItens;
  fimp.RLDBText66.DataSource := DSTmpItens;
  fimp.RLDBText67.DataSource := DSTmpItens;
  fimp.RLDBText68.DataSource := DSTmpItens;
  fimp.RLDBText69.DataSource := DSTmpItens;
  fimp.RLDBText70.DataSource := DSTmpItens;
  fimp.RLDBText71.DataSource := DSTmpItens;
  fimp.RLDBText72.DataSource := DSTmpItens;
  fimp.RLDBText73.DataSource := DSTmpItens;
  fimp.RLDBText75.DataSource := DSTmpItens;
  fimp.RLSubDetail3.DataSource := DSqryConsulta;
  fimp.RLDBText29.DataSource := DSqryConsulta;
  fimp.RLDBText30.DataSource := DSqryConsulta;
  fimp.RLDBText31.DataSource := DSqryConsulta;

  fimp.RLSubDetail6.DataSource := DSqryConsulta01;
  fimp.RLDBText80.DataSource := DSqryConsulta01;
  fimp.RLDBText81.DataSource := DSqryConsulta01;
  fimp.RLDBText82.DataSource := DSqryConsulta01;

  RLPrinter.Copies := 1;
  if not Parametro.PrintDialog then
  begin
    RLPrinter.PrinterName := '';
    fimp.RLReport3.PrintDialog := true;
  end
  else
  begin
    fimp.RLReport3.PrintDialog := false;
    RLPrinter.PrinterName := Parametro.impressora;
  end;
  fimp.RLLabel83.Caption := GEmitente.Fantasia;
  fimp.RLLabel84.Caption := Label11.Caption + '                ' + Label12.Caption + '                ' + 'No.: ' + StaticText2.Caption;
  fimp.RLLabel85.Caption := 'Tipo: ' + Copy(FMetodoPagto, 3, 5) + '  VENDEDOR: ' + StaticText5.Caption;

  if Label9.Caption = '1' then
  begin
    fimp.RLLabel85.Caption := 'Cliente: ' + Copy(lblConsumidor.Caption, 1, 22);
  end
  else
  begin
    fimp.RLLabel86.Caption := 'Cliente: ' + Copy(lblConsumidor.Caption, 1, 22) + '  Tel.: ' + tel;
    fimp.RLLabel87.Caption := 'End.: ' + ende + ', No.: ' + num;
    fimp.RLLabel88.Caption := 'Bairro: ' + bairro + '     Cidade: ' + cidade + '   ' + sigla;
    if Parametro.OS then
    begin
      fimp.RLLabel116.Visible := true;
      // fimp.RLLabel116.Caption := 'PLACA: ' + edtPlaca.Text + '  CARRO: ' + edtCarro.Text + '  KM: ' + edtKM.Text;
    end
    else
    begin
      fimp.RLLabel116.Visible := false;
    end;
  end;

  chama_temp;

  { fimp.RLLabel313.Caption := lblConsumidor.Caption;
    //  if Parametro.QTDE_FRACIONADA then
    //    fimp.RLLabel313.Caption := FormatFloat(MaskFloat, FQtdeItensVenda);

    fimp.RLLabel315.Caption := Label18.Caption;

    // if desconto1 = 0 then
    // Fimp.RLLabel316.Caption := '0,00'
    // else
    //fimp.RLLabel316.Caption := FormatFloat('0.00', ValorValido(Label20.Caption, 0) + desconto1);
    fimp.RLLabel316.Caption := FormatFloat('0.00', desconto);

    fimp.RLLabel317.Caption := FormatFloat('0.00', ValorValido(Label16.Caption, 0) - desconto1); // desconto desmembra
    fimp.RLLabel318.Caption := FormatFloat('0.00', StrToFloat(troco));
    if valor_recebido = 0 then
    fimp.RLLabel323.Caption := Label14.Caption
    else
    fimp.RLLabel323.Caption := FormatFloat('0.00', valor_recebido); }

  fimp.RLLabel313.Caption := Label14.Caption;
  fimp.RLLabel315.Caption := transform(rSUBT);
  fimp.RLLabel316.Caption := transform(rDESCO);
  if ComboBox1.ItemIndex = 2 then
    fimp.RLLabel317.Caption := Label16.Caption
  else
    fimp.RLLabel317.Caption := transform(rSUBT - rDESCO); // transform(rAPAGAR);
  fimp.RLLabel323.Caption := transform(rValorPago);
  fimp.RLLabel318.Caption := transform(rTROCO);

  qryConsulta.Close;
  qryConsulta.SQL.Clear;
  qryConsulta.SQL.Text := 'select numero, ordem, vencimento, valor, ESPECIE from receber where numero = :d0';
  qryConsulta.ParamByName('d0').AsString := StaticText2.Caption;
  qryConsulta.open;
  if qryConsulta.IsEmpty then
  begin
    fimp.RLSubDetail3.Visible := false;
    fimp.RLBand19.Visible := true;
  end
  else
  begin
    fimp.RLSubDetail3.Visible := true;
    fimp.RLBand19.Visible := false;
  end;

  if Parametro.especie_pgto then
  begin
    qryConsulta01.Close;
    qryConsulta01.SQL.Clear;
    qryConsulta01.SQL.Text := 'SELECT R.id_forma_pagamento, F.DESCRICAO, R.VALOR FROM recebimento_vendas R, forma_pagamento F WHERE R.id_forma_pagamento = F.id AND R.NOTA = :NOTA  ORDER BY R.id_forma_pagamento';
    qryConsulta01.ParamByName('NOTA').AsString := StaticText2.Caption;
    qryConsulta01.open;
    if qryConsulta01.IsEmpty then
    begin
      fimp.RLSubDetail6.Visible := false;
    end
    else
    begin
      fimp.RLSubDetail6.Visible := true;
    end;
  end;

  if not Parametro.PARCELA then
  begin
    fimp.RLBand15.Visible := false;
    fimp.RLBand17.Visible := false;
    fimp.RLBand18.Visible := false;
  end
  else
  begin
    fimp.RLBand15.Visible := true;
    fimp.RLBand17.Visible := true;
    fimp.RLBand18.Visible := true;
  end;

  if not Parametro.VEICULO then
  begin
    fimp.RLMemo2.Visible := false;
  end
  else
  begin
    fimp.RLMemo2.Visible := true;
    fimp.RLBand23.Visible := true;
  end;

  fimp.RLReport3.Margins.RightMargin := ValorValido(DM1.MARGEMDIREITA, 0);
  fimp.RLReport3.Margins.LeftMargin := ValorValido(DM1.MARGEMESQUERDA, 0);
  // fimp.RLReport3.PageSetup.Orientation := poPortrait(0);
  fimp.RLLabel117.Caption := GEmitente.Telefone;
  fimp.rllabel118.Caption := GEmitente.Endereco + ' ' + GEmitente.numero + ' ' + GEmitente.cidade + ' ' + GEmitente.UF;

  if parametro.IMPRIMIR_QRCODE_PIX then
     begin
       fimp.RLPIX.Picture.LoadFromFile('C:\DIGISAT\SUITEG5\PEDIDOS\LOGO\PIX.JPG');
       Fimp.RLBand56.Visible :=True;
       fimp.RLCNPJPix.Caption := gemitente.CPFCNPJ;
     end
     else
     begin
       Fimp.RLBand56.Visible := false;
     end;

  fimp.RLReport3.Print;
//  fimp.RLReport3.PreviewModal;

  FreeAndNil(fimp);
end;

function TFbalcao.Prazo: Boolean;
begin
  finaliza := '0';

  result := AbreParcelamento(FNota, StrToIntDef(Label9.Caption, 0), StrToIntDef('0', 0), ValorValido(Label16.Caption, 0), 0);

  if Fbalcao.CanFocus then
    Fbalcao.SetFocus;
end;

Procedure TFbalcao.SetDefaultPrinter(PrinterName: String);
var
  I: Integer;
  Device: PChar;
  Driver: PChar;
  Port: PChar;
  HdeviceMode: Thandle;
  aPrinter: TPrinter;
begin
  Printer.PrinterIndex := -1;
  getmem(Device, 255);
  getmem(Driver, 255);
  getmem(Port, 255);
  aPrinter := TPrinter.Create;
  for I := 0 to Printer.Printers.Count - 1 do
  begin
    if Printer.Printers[I] = PrinterName then
    begin
      aPrinter.PrinterIndex := I;
      aPrinter.getprinter(Device, Driver, Port, HdeviceMode);
      StrCat(Device, ',');
      StrCat(Device, Driver);
      StrCat(Device, Port);
      WriteProfileString('windows', 'device', Device);
      StrCopy(Device, 'windows');
      SendMessage(HWND_BROADCAST, WM_WININICHANGE, 0, Longint(@Device));
    end;
  end;
  Freemem(Device, 255);
  Freemem(Driver, 255);
  Freemem(Port, 255);
  aPrinter.Free;
end;

function TFbalcao.trocaImpressoraPadrao(pNomeNovaImpressora: string): Boolean;
var
  W2KSDP: function(pszPrinter: PChar): Boolean; stdcall;
  H: Thandle;
  Size, Dummy: Cardinal;
  PI: PPrinterInfo2;
begin
  result := false;
  try
    if pNomeNovaImpressora <> '' then
    begin
      if (Win32Platform = VER_PLATFORM_WIN32_NT) and (Win32MajorVersion >= 5) then
      begin
        @W2KSDP := GetProcAddress(GetModuleHandle(winspl), 'SetDefaultPrinterA');

        if (@W2KSDP <> nil) and (W2KSDP(PChar(pNomeNovaImpressora))) then
          result := true
        else
          RaiseLastOSError;
      end
      else
      begin
        if OpenPrinter(PChar(pNomeNovaImpressora), H, nil) then
          try
            WinSpool.getprinter(H, 2, nil, 0, @Size);

            if GetLastError <> ERROR_INSUFFICIENT_BUFFER then
              RaiseLastOSError;

            getmem(PI, Size);
            try
              if WinSpool.getprinter(H, 2, PI, Size, @Dummy) then
              begin
                PI^.Attributes := PI^.Attributes or PRINTER_ATTRIBUTE_DEFAULT;
                if WinSpool.SetPrinter(H, 2, PI, PRINTER_CONTROL_SET_STATUS) then
                  result := true
                else
                  RaiseLastOSError;
              end
              else
                RaiseLastOSError;
            finally
              Freemem(PI);
            end;
          finally
            ClosePrinter(H);
          end
        else
          RaiseLastOSError;
      end;
    end;
  except
    raise;
  end;
end;

procedure TFbalcao.trocaimpressora;
var
  impressoraPadraoAntiga: string;
begin
  impressoraPadraoAntiga := Printer.Printers[Printer.PrinterIndex];
  trocaImpressoraPadrao(Parametro.impressora);
  try
    // process...
  finally
    trocaImpressoraPadrao(impressoraPadraoAntiga);
  end;
end;

function TFbalcao.GetDefaultPrinterName: string;
//
// Retorna o nome da impressora padrão do Windows
//
begin
  if (Printer.PrinterIndex >= 0) then
  begin
    result := Printer.Printers[Printer.PrinterIndex];
  end
  else
  begin
    result := 'Nenhuma impressora Padrão foi detectada';
  end;
end;

procedure TFbalcao.itens_zerado;
begin
  qryItensTeste.Close;
  qryItensTeste.SQL.Clear;
  qryItensTeste.SQL.add('select CUPOM, item , descricao from frente_tmpitvendas where cupom = :nota and tipo = 1 ');
  qryItensTeste.ParamByName('nota').AsString := StaticText2.Caption;
  qryItensTeste.open;

  if qryItensTeste.IsEmpty then
  begin
    MessageDlg('VENDA NÃO SERÁ FECHADA, ITEM COM VALOR ZERADO! ITEM: ' + qryItensTeste.FieldByName('ITEM').AsString + '  ' + qryItensTeste.FieldByName('DESCRICAO').AsString, mtWarning, [mbOK], 0);
    exit;
  end;
end;

procedure TFbalcao.ConfiguraSAT_01;
var
  ModeloSAT: TSatModelo;
begin
  dm1.qryconsulta.Close;
  dm1.qryconsulta.sql.Text := 'select CONTINGENCIA,PORTA,MODELO,NVIAS,' + 'IMPRIME,USAGAVETA,VELOCIDADE from VENDAS_TERMINAIS ' + 'where NOME=' + QuotedStr(dm1.Getcomputer);
  dm1.qryconsulta.Open;

  DiretoriosDeArquivos;

  dm1.qryConfig.Close;
  dm1.qryConfig.Params[0].AsInteger := GEmitente.IDEmitente;
  dm1.qryConfig.Open;

  if dm1.qryConfig.IsEmpty then
    raise Exception.Create('Módulo DF-e ainda não foi configurado, impossível continuar!');

  ACBrSAT1.DesInicializar;
  if not dm1.qryConfig.IsEmpty then
  begin
    ModeloSAT := dm1.GetModeloSAT(dm1.qryConfigMODELO_DLL.AsString);

    { if ModeloSAT.Tipo = mfe_Integrador_XML then
      ACBrSAT1.Integrador := ACBrIntegrador1; }
    ACBrSAT1.OnGetsignAC := GetsignAC;
    ACBrSAT1.OnGetcodigoDeAtivacao := GetcodigoDeAtivacao;
    ACBrSAT1.Modelo := ModeloSAT.tipo;
    ACBrSAT1.NomeDLL := ModeloSAT.PathDll;
    ACBrSAT1.Config.ide_CNPJ := TiraPontos(dm1.qryConfigSAT_CNPJ.AsString);
    ACBrSAT1.Config.ide_numeroCaixa := StrToInt(dm1.Terminal);
    ACBrSAT1.Config.emit_CNPJ := TiraPontos(GEmitente.CPFCNPJ);
    ACBrSAT1.Config.emit_IE := TiraPontos(GEmitente.InscRG);
    ACBrSAT1.Config.emit_IM := TiraPontos(GEmitente.InscMunicipal);
    ACBrSAT1.Config.emit_cRegTribISSQN := RTISSMicroempresaMunicipal;
    ACBrSAT1.Config.emit_indRatISSQN := irSim;
    ACBrSAT1.Config.PaginaDeCodigo := ModeloSAT.PaginaCodigo;
    ACBrSAT1.Config.EhUTF8 := ModeloSAT.UTF8;
    ACBrSAT1.Config.infCFe_versaoDadosEnt := StrToFloatDef(dm1.qryConfigCFE_VERSAO.AsString, 0.07);

    if Trim(GEmitente.Regime) = 'SIMPLES NACIONAL' then
      ACBrSAT1.Config.emit_cRegTrib := RTSimplesNacional
    else
      ACBrSAT1.Config.emit_cRegTrib := RTRegimeNormal;

    ACBrSAT1.ConfigArquivos.SalvarCFe := true;
    ACBrSAT1.ConfigArquivos.SalvarCFeCanc := true;
    ACBrSAT1.ConfigArquivos.SalvarEnvio := False;
    ACBrSAT1.ConfigArquivos.SepararPorCNPJ := true;
    ACBrSAT1.ConfigArquivos.SepararPorMes := true;

    // diretorios onde salvar os arquivos
    ACBrSAT1.ConfigArquivos.PastaCFeVenda := PathArquivos;
    ACBrSAT1.ConfigArquivos.PastaCFeCancelamento := PathArquivos;
    ACBrSAT1.ConfigArquivos.PastaEnvio := PathTmp;

    ACBrSAT1.CFe.IdentarXML := False;
    ACBrSAT1.CFe.TamanhoIdentacao := 1;

    // Rede
    if (Trim(dm1.qryConfigSAT_USUARIO.AsString) <> '') or (Trim(dm1.qryConfigSAT_LANIP.AsString) <> '') then
    begin
      ACBrSAT1.Rede.tipoInter := TTipoInterface(dm1.qryConfigSAT_TIPO_INTER.AsInteger);
      ACBrSAT1.Rede.SSID := dm1.qryConfigSAT_SSID.AsString;
      ACBrSAT1.Rede.seg := TSegSemFio(dm1.qryConfigSAT_SEG.AsInteger);
      ACBrSAT1.Rede.codigo := dm1.qryConfigSAT_CODIGO.AsString;
      ACBrSAT1.Rede.tipoLan := TTipoLan(dm1.qryConfigSAT_TIPOLAN.AsInteger);
      ACBrSAT1.Rede.lanIP := dm1.qryConfigSAT_LANIP.AsString;
      ACBrSAT1.Rede.lanMask := dm1.qryConfigSAT_LANMASK.AsString;
      ACBrSAT1.Rede.lanGW := dm1.qryConfigSAT_LANGW.AsString;
      ACBrSAT1.Rede.lanDNS1 := dm1.qryConfigSAT_LANDNS1.AsString;
      ACBrSAT1.Rede.lanDNS2 := dm1.qryConfigSAT_LANDNS2.AsString;
      ACBrSAT1.Rede.usuario := dm1.qryConfigSAT_USUARIO.AsString;
      ACBrSAT1.Rede.senha := dm1.qryConfigSAT_SENHA.AsString;
      ACBrSAT1.Rede.proxy := dm1.qryConfigSAT_PROXY.AsInteger;
      ACBrSAT1.Rede.proxy_ip := dm1.qryConfigSAT_PROXY_IP.AsString;
      ACBrSAT1.Rede.proxy_porta := dm1.qryConfigSAT_PROXY_PORTA.AsInteger;
      ACBrSAT1.Rede.proxy_user := dm1.qryConfigSAT_PROXY_USER.AsString;
      ACBrSAT1.Rede.proxy_senha := dm1.qryConfigSAT_PROXY_SENHA.AsString;
    end;

    ACBrSAT1.Inicializar;
  end;

  // configurações impressão escpos

  if dm1.qryTerminalTIPOIMPRESSORA.AsString = '1' then
  begin
    ACBrSAT1.Extrato := ACBrSATExtratoFortes1;
    ACBrSATExtratoFortes1.Impressora := dm1.qryTerminalPORTA.Value;
  end
  else
  begin
    ACBrSAT1.Extrato := ACBrSATExtratoESCPOS1;

    ACBrPosPrinter1.EspacoEntreLinhas := dm1.qryTerminalESPACO_ENTRE_LINHAS.AsInteger;
    ACBrPosPrinter1.LinhasEntreCupons := dm1.qryTerminalLINHAS_ENTRE_CUPOM.AsInteger;

    ACBrSATExtratoESCPOS1.MargemDireita := dm1.qryTerminalMARGEM_DIREITA.AsFloat;
    ACBrSATExtratoESCPOS1.MargemEsquerda := dm1.qryTerminalMARGEM_ESQUERDA.AsFloat;
    ACBrSATExtratoESCPOS1.MargemInferior := dm1.qryTerminalMARGEM_INFERIOR.AsFloat;
    ACBrSATExtratoESCPOS1.MargemSuperior := dm1.qryTerminalMARGEM_SUPERIOR.AsFloat;

    ACBrPosPrinter1.Porta := LowerCase(dm1.qryTerminal.FieldByName('PORTA').AsString);
    ACBrPosPrinter1.Device.Baud := dm1.qryTerminalVELOCIDADE.Value;
    ACBrPosPrinter1.PaginaDeCodigo := StrToPaginaCodigo(dm1.qryTerminalPAGINA_CODIGO.AsString);

    if dm1.qryTerminal.FieldByName('MODELO').Value = 'DARUMA' then
      ACBrPosPrinter1.Modelo := ppEscDaruma
    else
      if dm1.qryTerminal.FieldByName('MODELO').Value = 'BEMATECH' then
        ACBrPosPrinter1.Modelo := ppEscBematech
      else
        if dm1.qryTerminal.FieldByName('MODELO').Value = 'ELGIN' then
          ACBrPosPrinter1.Modelo := ppEscPosEpson
        else
          if dm1.qryTerminal.FieldByName('MODELO').Value = 'DIEBOLD' then
            ACBrPosPrinter1.Modelo := ppEscDiebold
          else
            if dm1.qryTerminal.FieldByName('MODELO').Value = 'EPSON' then
              ACBrPosPrinter1.Modelo := ppEscPosEpson
            else
              if dm1.qryTerminal.FieldByName('MODELO').Value = 'VOX' then
                ACBrPosPrinter1.Modelo := ppEscVox
              else
                if dm1.qryTerminal.FieldByName('MODELO').Value = 'POSSTAR' then
                  ACBrPosPrinter1.Modelo := ppEscPosStar
                else
                  if dm1.qryTerminal.FieldByName('MODELO').Value = 'JIANG' then
                    ACBrPosPrinter1.Modelo := ppEscZJiang
                  else
                    if dm1.qryTerminal.FieldByName('MODELO').Value = 'GPRINTER' then
                      ACBrPosPrinter1.Modelo := ppEscGPrinter
                    else
                      if dm1.qryTerminal.FieldByName('MODELO').Value = 'EPSONP2' then
                        ACBrPosPrinter1.Modelo := ppEscEpsonP2
                      else
                        ACBrPosPrinter1.Modelo := ppTexto;

    ACBrPosPrinter1.Ativar;

  end;

  ACBrSAT1.Extrato.Sistema := 'IN9VE SISTEMAS';

  ACBrSAT1.Extrato.ImprimeEmUmaLinha := False;
  if dm1.qryTerminalIMPRIME_DUAS_LINHAS.Value = 'S' then
    ACBrSAT1.Extrato.ImprimeEmUmaLinha := true;

  ACBrSAT1.Extrato.PathPDF := PathPDF;
  ACBrSAT1.Extrato.ImprimeDescAcrescItem := False;
  ACBrSAT1.Extrato.ImprimeCodigoEan := true;

  if FilesExists(dm1.qryConfigLOGOMARCA.AsString) then
    ACBrSAT1.Extrato.Logo := dm1.qryConfigLOGOMARCA.AsString;

  ACBrSAT1.Extrato.Site := ''; // #VERIFICAR
  ACBrSAT1.Extrato.Email := '';
  ACBrSAT1.Extrato.MostraPreview := ACBrSATExtratoFortes1.Impressora.IsEmpty;
end;

procedure TFbalcao.valepresente;
var
  valorcheio : Real;
  nImpressao : Integer;
  ncontador : Integer;
begin
  valorcheio := 0;
  ncontador := 1;
  nImpressao := 0;
  valorcheio := StrToFloat(label51.Caption);

  if valorcheio <= 0 then
  begin
    ShowMessage('O valor deve ser maior que zero.');
    Exit;
  end;

  if valorcheio > 99.99 then
    begin
      frmPreencheValePresente := TfrmPreencheValePresente.create(application);
      try
        frmPreencheValePresente.showmodal;
      except
        FreeAndNil(frmPreencheValePresente);
      end;

      //nImpressao := Round(valorcheio / 100);
      nImpressao := Floor(valorcheio / 100);

      for ncontador := 1 to nImpressao do
      begin
        Fimp := TFimp.Create(Application);
        try
          RLPrinter.PrinterName := Parametro.impressora;
          fimp.RLReport12.PrintDialog := False;

          fimp.pplabelNome.Caption := vpnome;
          fimp.pplabelTelefone.Caption := vptelefone ;
          fimp.pplabelVendedor.Caption := edit3.Text + '  ' + StaticText5.Caption;

          fimp.RLReport12.Print;
          //fimp.RLReport12.PreviewModal;
        except
          FreeAndNil(fimp);
        end;
      end;
  end;
end;

procedure TFbalcao.AjustaACBrSAT;
var
  ModeloSAT: TSatModelo;
begin
  with dm1.ACBrSAT1 do
  begin
    ModeloSAT := DM1.GetModeloSAT(DM1.qryConfigMODELO_DLL.AsString);
    DM1.AcbrSAT1.OnGetsignAC := GetsignAC;
    DM1.AcbrSAT1.OnGetcodigoDeAtivacao := GetcodigoDeAtivacao;
    DM1.AcbrSAT1.Modelo := ModeloSAT.Tipo;
    DM1.AcbrSAT1.NomeDLL := ModeloSAT.PathDll;
    DM1.AcbrSAT1.Config.ide_numeroCaixa := StrToInt(DM1.TERMINAL);
    DM1.AcbrSAT1.Config.ide_CNPJ := TiraPontos(DM1.qryConfigSAT_CNPJ.AsString);
    DM1.AcbrSAT1.Config.emit_CNPJ := TiraPontos(GEmitente.CPFCNPJ);
    DM1.AcbrSAT1.Config.emit_IE := TiraPontos(GEmitente.InscRG);
    DM1.AcbrSAT1.Config.emit_IM := TiraPontos(GEmitente.InscMunicipal);
    DM1.AcbrSAT1.Config.emit_cRegTribISSQN := RTISSMicroempresaMunicipal;
    DM1.AcbrSAT1.Config.emit_indRatISSQN := irSim;
    DM1.AcbrSAT1.Config.PaginaDeCodigo := ModeloSAT.PaginaCodigo;
    DM1.AcbrSAT1.Config.EhUTF8 := ModeloSAT.UTF8;
    DM1.AcbrSAT1.Config.infCFe_versaoDadosEnt := StrToFloatDef(DM1.qryConfigCFE_VERSAO.AsString, 0.07);

    DM1.AcbrSAT1.ConfigArquivos.SalvarCFe := true;
    DM1.AcbrSAT1.ConfigArquivos.SalvarCFeCanc := true;
    DM1.AcbrSAT1.ConfigArquivos.SalvarEnvio := false;
    DM1.AcbrSAT1.ConfigArquivos.SepararPorCNPJ := true;
    DM1.AcbrSAT1.ConfigArquivos.SepararPorModelo := false;
    DM1.AcbrSAT1.ConfigArquivos.SepararPorDia := false;
    DM1.AcbrSAT1.ConfigArquivos.SepararPorMes := true;
    DM1.AcbrSAT1.ConfigArquivos.SepararPorAno := false;

    dm1.ACBrSAT1.Inicializar;
//    if DM1.ACBrSAT1.Inicializado := not DM1.ACBrSAT1.Inicializado;
  end
end;

function TFbalcao.LimparDescricao(const descricao: string): string;
var
  i: Integer;
  c: Char;
begin
  Result := '';
  for i := 1 to Length(descricao) do
  begin
    c := descricao[i];
    // Verifica se o caractere é uma letra, número ou espaço em branco
    if CharInSet(c, ['A'..'Z', 'a'..'z', '0'..'9', ' ']) then
      Result := Result + c
    else
      Result := Result + ' '; // Substitui o caractere especial por espaço
  end;
end;

function TFBalcao.NomeComputadorApi: String;
var
  lSize : Cardinal;
  lComputerName: Array [0 .. Max_Path] of Char;
begin
  lSize := max_path;
  GetComputerName(lComputerName, lSize);
  Result := lComputerName;
end;


procedure TFBalcao.sDuplicidade;
begin
  if not(DM1.qryNFCE_M.State in dsEditModes) then
    DM1.qryNFCE_M.Edit;
  DM1.qryNFCE_MFLAG.Value := 'N';
  DM1.qryNFCE_MABERTO.Value := 'N';
  DM1.qryNFCE_MSITUACAO.Value := 'D';
  DM1.qryNFCE_MDATA_EMISSAO.Value := DATE;
  DM1.qryNFCE_MDATA_SAIDA.Value := DATE;
  DM1.qryNFCE_MHORA_EMISSAO.Value := now;
  DM1.qryNFCE_MHORA_SAIDA.Value := now;
  DM1.qryNFCE_MFK_VENDA.Value :=StrToInt(StaticText2.Caption);
  DM1.qryNFCE_M.Post;
 if DM1.qryNFCE_M.Transaction.Active then
      DM1.qryNFCE_M.Transaction.Commit;



  ShowMessage('Retorno:' + DM1.ACBrNFe1.WebServices.Enviar.cStat.ToString + ' - ' +
    DM1.ACBrNFe1.WebServices.Enviar.xMotivo);
end;

procedure TFBalcao.sDenegada;
begin
  if not(DM1.qryNFCE_M.State in dsEditModes) then
    DM1.qryNFCE_M.Edit;
  DM1.qryNFCE_MFLAG.Value := 'N';
  DM1.qryNFCE_MABERTO.Value := 'N';
  DM1.qryNFCE_MSITUACAO.Value := 'X';
  DM1.qryNFCE_MDATA_EMISSAO.Value := DATE;
  DM1.qryNFCE_MDATA_SAIDA.Value := DATE;
  DM1.qryNFCE_MHORA_EMISSAO.Value := now;
  DM1.qryNFCE_MHORA_SAIDA.Value := now;
  DM1.qryNFCE_MFK_VENDA.Value := StrToInt(StaticText2.Caption);
  DM1.qryNFCE_M.Post;
 if DM1.qryNFCE_M.Transaction.Active then
      DM1.qryNFCE_M.Transaction.Commit;

  ShowMessage('Retorno:' + DM1.ACBrNFe1.WebServices.Enviar.cStat.ToString + ' - ' +
    DM1.ACBrNFe1.WebServices.Enviar.xMotivo);
end;

procedure TFBalcao.sCancelada;
begin

  if not(DM1.qryNFCE_M.State in dsEditModes) then
    DM1.qryNFCE_M.Edit;
  DM1.qryNFCE_MCHAVE.Value :=
    copy(DM1.ACBrNFe1.NotasFiscais.Items[0].NFe.infNFe.ID, 4, 100);
  DM1.qryNFCE_MPROTOCOLO.Value := DM1.ACBrNFe1.NotasFiscais.Items[0]
    .NFe.procNFe.nProt;
  DM1.qryNFCE_MXML.Value := DM1.ACBrNFe1.NotasFiscais.Items[0].XML;
  DM1.qryNFCE_MFLAG.Value := 'N';
  DM1.qryNFCE_MABERTO.Value := 'N';
  DM1.qryNFCE_MSITUACAO.Value := 'C';
  DM1.qryNFCE_MFK_VENDA.Value := StrToInt(StaticText2.Caption);
  DM1.qryNFCE_M.Post;
  if DM1.qryNFCE_M.Transaction.Active then
      DM1.qryNFCE_M.Transaction.Commit;

  ShowMessage('Retorno:' + DM1.ACBrNFe1.WebServices.Enviar.cStat.ToString + ' - ' +
    DM1.ACBrNFe1.WebServices.Enviar.xMotivo);
end;

procedure TFBalcao.sTransmitida;
begin
  // atualiza status da nfce
  if DM1.ACBrNFe1.WebServices.Enviar.cStat = 100 then
  begin
    DM1.ACBrNFe1.NotasFiscais.Items[0].GravarXML('');
    if not(DM1.qryNFCE_M.State in dsEditModes) then
      DM1.qryNFCE_M.Edit;
    DM1.qryNFCE_MCHAVE.Value :=
      copy(DM1.ACBrNFe1.NotasFiscais.Items[0].NFe.infNFe.ID, 4, 100);
    DM1.qryNFCE_MPROTOCOLO.Value := DM1.ACBrNFe1.NotasFiscais.Items[0]
      .NFe.procNFe.nProt;
    DM1.qryNFCE_MXML.Value := DM1.ACBrNFe1.NotasFiscais.Items[0].XML;
    DM1.qryNFCE_MFLAG.Value := 'N';
    DM1.qryNFCE_MABERTO.Value := 'N';
    DM1.qryNFCE_MSITUACAO.Value := 'T';
    DM1.qryNFCE_MDATA_EMISSAO.Value := DATE;
    DM1.qryNFCE_MDATA_SAIDA.Value := DATE;
    DM1.qryNFCE_MHORA_EMISSAO.Value := now;
    DM1.qryNFCE_MHORA_SAIDA.Value := now;
    DM1.qryNFCE_MFK_VENDA.Value := StrToInt(StaticText2.Caption);
    DM1.qryNFCE_M.Post;
    if DM1.qryNFCE_M.Transaction.Active then
      DM1.qryNFCE_M.Transaction.Commit;



//    if DM1.qryTerminalUSAGAVETA.Value = 'S' then
//    // abre gaveta
//    begin
//      DM1.ACBrNFe1.DANFE := ACBrNFeDANFeESCPOS1;
//      ACBrPosPrinter1.AbrirGaveta;
//    end;


    if DM1.qryTerminalIMPRIME.AsString = 'S' then
      DM1.ACBrNFe1.NotasFiscais.Imprimir
    else
    begin
      if Application.messageBox(Pwidechar('Deseja Imprimir NFC-e?'),
        'Confirmação', mb_Yesno) = mrYes then
        DM1.ACBrNFe1.NotasFiscais.Imprimir;
    end;
  end;
end;

procedure TFBalcao.ConfiguraNFCe;
var
  Ok: boolean;
begin
  DM1.qryTerminal.Close;
  DM1.qryTerminal.open;
  DM1.qryTerminal.Locate('nome', DM1.NomeTerminal, []);

  ConfiguraImpressora(DM1.qryTerminalTIPOIMPRESSORA.AsString);

  dm1.qryConfig.Close;
  dm1.qryConfig.Params[0].Value := GEmitente.IDEmitente;
  dm1.qryConfig.Open;
//  dm1.qryConfig.Edit;

  dm1.qryConsulta.Close;
  dm1.qryConsulta.SQL.Text := 'select CONTINGENCIA,PORTA,MODELO,NVIAS,IMPRIME,USAGAVETA from VENDAS_TERMINAIS where NOME=' + QuotedStr(dm1.Getcomputer);
  dm1.qryConsulta.Open;

  DiretoriosDeArquivos;

  if (dm1.qryConfig.IsEmpty) then
  begin
    ShowMessage('Configure os parametros para emissão do NFCe!');
    exit;
  end;

  with DM1.ACBrNFe1.Configuracoes.Geral do // configurações gerais
  begin
    ExibirErroSchema := false;
    IdCSC := dm1.qryConfigIDTOKEN.AsString;
    CSC := dm1.qryConfigTOKEN.AsString;

    if dm1.qryConfigVISUALIZAERROSCHEMA.Value = 'S' then
      ExibirErroSchema := true;
    FormatoAlerta := '[ %TAGNIVEL%%TAG% ]   %DESCRICAO% - %MSG%'; //dm1.qryConfigFORMATOALERTA.Value;
    FormaEmissao := TpcnTipoEmissao(teNormal);
    ModeloDF := TpcnModeloDF(moNFCe);
    VersaoDF := TpcnVersaoDF(dm1.qryConfigVERSAODF.Value);

    CamposFatObrigatorios := false;
    if dm1.qryConfigAMBIENTE.Value = 1 then // homollogação
      CamposFatObrigatorios := true;

    Salvar := false;
    SSLLib := TSSLLib(dm1.qryConfigTIPO_EMISSAO.Value);
    SSLCryptLib := TSSLCryptLib(dm1.qryConfigCRYPTLIB.AsInteger);
    SSLHttpLib := TSSLHttpLib(dm1.qryConfigHTTPLIB.AsInteger);
    SSLXmlSignLib := TSSLXmlSignLib(dm1.qryConfigXMLSIGN.AsInteger);
    Salvar := true;
  end;

  // certificado
  DM1.ACBrNFe1.Configuracoes.Certificados.Senha := dm1.qryConfigSENHACERTIFICADO.Value;
  DM1.ACBrNFe1.Configuracoes.Certificados.ArquivoPFX := dm1.qryConfigCAMINHO_CERTIFICADO.Value;
  DM1.ACBrNFe1.Configuracoes.Certificados.NumeroSerie := dm1.qryConfigNUMEROSERIECERTFICADO.Value;

  // DM1.ACBrNFe1.SSL.CarregarCertificado;


   DM1.ACBrNFe1.Configuracoes.WebServices.UF := dm1.qryConfigUF.AsString;
    DM1.ACBrNFe1.Configuracoes.WebServices.Ambiente := StrToTpAmb(Ok, IntToStr(dm1.qryConfigAMBIENTE.Value + 1));

    DM1.ACBrNFe1.Configuracoes.WebServices.Visualizar := false;
    if dm1.qryConfigVISUALIZAR.Value = 'S' then
      DM1.ACBrNFe1.Configuracoes.WebServices.Visualizar := true;

    DM1.ACBrNFe1.Configuracoes.WebServices.Salvar := false;
    if dm1.qryConfigSALVARSOAP.Value = 'S' then
      DM1.ACBrNFe1.Configuracoes.WebServices.Salvar := true;

    DM1.ACBrNFe1.Configuracoes.WebServices.AjustaAguardaConsultaRet := false;
    if dm1.qryConfigAJUSTARAUTO.Value = 'S' then
      DM1.ACBrNFe1.Configuracoes.WebServices.AjustaAguardaConsultaRet := true;

    if NaoEstaVazio(dm1.qryConfigAGUARDAR.AsString) then
      DM1.ACBrNFe1.Configuracoes.WebServices.AguardarConsultaRet := ifThen(StrToInt(dm1.qryConfigAGUARDAR.AsString) < 1000, StrToInt(dm1.qryConfigAGUARDAR.AsString) * 1000, StrToInt(dm1.qryConfigAGUARDAR.AsString))
    else
    begin
      dm1.qryConfig.Edit;
      dm1.qryConfigAGUARDAR.AsString := IntToStr(DM1.ACBrNFe1.Configuracoes.WebServices.AguardarConsultaRet);
    end;

    if NaoEstaVazio(dm1.qryConfigTENTATIVAS.AsString) then
      DM1.ACBrNFe1.Configuracoes.WebServices.Tentativas := StrToInt(dm1.qryConfigTENTATIVAS.AsString)
    else
      dm1.qryConfigTENTATIVAS.AsString := IntToStr(DM1.ACBrNFe1.Configuracoes.WebServices.Tentativas);

    if NaoEstaVazio(dm1.qryConfigINTERVALO.AsString) then
      DM1.ACBrNFe1.Configuracoes.WebServices.IntervaloTentativas := ifThen(StrToInt(dm1.qryConfigINTERVALO.AsString) < 1000, StrToInt(dm1.qryConfigINTERVALO.AsString) * 1000, StrToInt(dm1.qryConfigINTERVALO.AsString))
    else
      dm1.qryConfigINTERVALO.AsString := IntToStr(DM1.ACBrNFe1.Configuracoes.WebServices.IntervaloTentativas);

    DM1.ACBrNFe1.Configuracoes.WebServices.ProxyHost := '';
    DM1.ACBrNFe1.Configuracoes.WebServices.ProxyPort := '';
    DM1.ACBrNFe1.Configuracoes.WebServices.ProxyUser := '';
    DM1.ACBrNFe1.Configuracoes.WebServices.ProxyPass := '';
    DM1.ACBrNFe1.SSL.SSLType := TSSLType(dm1.qryconfigSSL_TIPO.AsInteger);


  with DM1.ACBrNFe1.Configuracoes.Arquivos do
  // configura caminho dos arqivos
  begin
    Salvar := true;
    SepararPorMes := true;
    AdicionarLiteral := false;
    if dm1.qryConfigLITERAL.Value = 'S' then
      AdicionarLiteral := true;

    EmissaoPathNFe := true;
    SalvarEvento := true;
    SepararPorCNPJ := true;
    SepararPorModelo := false;

    PathSalvar := dm1.qryConfigPATHSALVARNFE.Value;
    PathSchemas := dm1.qryConfigPATHSCHEMAS_NFE.AsString;
    PathNFe := dm1.qryConfigPATHENVIADA_NFE.Value;
    PathInu := dm1.qryConfigPATHINUTI_NFE.Value;
    PathEvento := dm1.qryConfigPATHEVENTO.Value;
  end;

  if DM1.ACBrNFe1.DANFE <> nil then
  begin
     //if not dm1.qryConfigLOGOMARCA.IsNull then
     if Parametro.LOGO_NFE then
      DM1.ACBrNFe1.DANFE.logo := parametro.LOCAL_LOGO_NFE
      // 'C:\DIGISAT\SUITEG5\PEDIDOS\LOGO\LOGO.JPG' // dm1.qryConfigLOGOMARCA.Value;
      else DM1.ACBrNFe1.DANFE.Logo := '';

    DM1.ACBrNFe1.DANFE.PathPDF := dm1.qryConfigPATHPDF_NFE.Value;
    DM1.ACBrNFe1.DANFE.MargemEsquerda := 5;
    DM1.ACBrNFe1.DANFE.MargemDireita := 0.5;
  end;
  DM1.ACBrNFeDANFCEFR1.FastFile := ExtractFilePath(Application.ExeName) + '\Relatorios\DANFeNFCe.fr3';
  DM1.ACBrNFeDANFCEFR1.Sistema := 'IN9VE SISTEMAS';
  DM1.ACBrNFeDANFCEFR1.Site := ''; // #VERIFICAR

  if not dm1.qryConfigLOGOMARCA.IsNull then
    DM1.ACBrNFeDANFCEFR1.logo := dm1.qryConfigLOGOMARCA.Value;

  DM1.ACBrNFeDANFCEFR1.PathPDF := dm1.qryConfigPATHPDF_NFE.Value;
  DM1.ACBrNFeDANFCEFR1.MargemEsquerda := 5;
  DM1.ACBrNFeDANFCEFR1.MargemDireita := 0.5;
end;

procedure TFBalcao.VerificarArquivo(const CaminhoArquivo: string);
begin
  if FileExists(CaminhoArquivo) then
  begin
    LOGO_BALCAO_STATUS := true;
    //ShowMessage('O arquivo existe: ' + CaminhoArquivo);
  end
  else
  begin
    ShowMessage('Aviso: O arquivo não foi encontrado: ' + CaminhoArquivo);
    ShowMessage(' Favor criar o ARQUIVO, ou informar na Tabela Parametros que LOGO_BALCAO é falso.');
    LOGO_BALCAO_STATUS := false;
  end;
end;

//procedure TFBalcao.Imprimir_Cupom_Compacto;
//var
//  cidade, sigla: string[40];
//  compl, cep: string;
//  tel, ende, bairro, num, Fantasia: string;
//  codemitente: Integer;
//  PDF, PASTA: string;
//  ie, cnpj : string;
//begin
//  ende := '';
//  num := '';
//  bairro := '';
//  cidade := '';
//  sigla := '';
//  compl := '';
//  cep := '';
//
//  fimp1 := Tfimp1.Create(Application);
//
//  fimp1.ppDBText22.datapipiline := DSTmpItens;
//  fimp1.ppDBText23.datapapiline := DSTmpItens;
//  fimp1.ppDBText24.datapapiline := DSTmpItens;
//  fimp1.ppDBText25.datapapiline := DSTmpItens;
//  fimp1.ppDBText26.datapapiline := DSTmpItens;
//  fimp1.ppDBText27.datapapiline := DSTmpItens;
//  fimp1.ppDBText28.datapapiline := DSTmpItens;
//  fimp1.ppDBText29.datapapiline := DSTmpItens;
//
//  Fimp1.ppfantasia.Caption := GEmitente.Fantasia;
//  FIMP1.PPENDERECO.CAPTION := Gemitente.Endereco + ' , ' + GEmitente.Numero + ' - ' + GEmitente.Bairro;
//  fimp1.ppcnpj.caption := 'CNPJ: ' + GEmitente.CPFCNPJ + '      ' + 'IE: ' + GEmitente.InscRG ;
//  Fimp1.ppdata.Caption := lblData.Caption+ '                ' + lblHora.Caption + '                ' + 'No.: ' + StaticText2.Caption;
//  if lancado = '0' then
//  Fimp1.ppespecie.Caption := 'Tipo: PRE VENDA          VENDEDOR: ' + StaticText5.Caption else
//  Fimp1.ppespecie.Caption := 'Tipo: ' + copy(FMetodoPagto, 3, 5) + '  VENDEDOR: ' + StaticText5.Caption;
//
//  chama_temp;
//
//  fimp1.ppqtd.caption := formatfloat('0.00', StrToFloatDef(label14.Caption,0) );
//  fimp1.pptotal.caption := formatfloat('0.00', StrToFloatDef(label16.Caption,0) );
//  fimp1.pptotalpago.caption := formatfloat('0.00', StrToFloatDef(label14.Caption,0) );
//  fimp1.pptroco.caption := formatfloat('0.00', StrToFloatDef(label16.Caption,0) );
//
//  fimp1.ppcupom.print;
//end;

procedure TFBalcao.Imprimir_Cupom_Compacto;
var
  cidade, sigla: string[40];
  compl, cep: string;
  tel, ende, bairro, num, Fantasia: string;
  codemitente: Integer;
  PDF, PASTA: string;
  ie, cnpj : string;
begin
  ende := '';
  num := '';
  bairro := '';
  cidade := '';
  sigla := '';
  compl := '';
  cep := '';

  ppfantasia.Caption := GEmitente.Fantasia;
  PPENDERECO.CAPTION := Gemitente.Endereco + ' , ' + GEmitente.Numero + ' - ' + GEmitente.Bairro;
  ppcnpj.caption := 'CNPJ: ' + GEmitente.CPFCNPJ + '      ' + 'IE: ' + GEmitente.InscRG;

  // Ajuste de segurança para valores numéricos
  ppqtd.caption := FormatFloat('0.00', StrToFloatDef(Label14.Caption, 0));
  pptotal.caption := FormatFloat('0.00', StrToFloatDef(Label16.Caption, 0));
  pptotalpago.caption := FormatFloat('0.00', StrToFloatDef(Label14.Caption, 0));
  pptroco.caption := FormatFloat('0.00', StrToFloatDef(Label16.Caption, 0));

  // Impressão do relatório
  ppcupom.Print;
end;

function TFBalcao.Getcomputer: String;
var
  lpBuffer: PChar;
  nSize: DWord;
const
  Buff_Size = MAX_COMPUTERNAME_LENGTH + 1;
begin
  nSize := Buff_Size;
  lpBuffer := StrAlloc(Buff_Size);
  GetComputerName(lpBuffer, nSize);
  result := String(lpBuffer);
  StrDispose(lpBuffer);
end;

end.
