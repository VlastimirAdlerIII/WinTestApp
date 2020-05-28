using System;
using System.Collections;
using System.Text;


namespace AutoCont.Barcodes
{
    /// <summary>
    /// specializovaná třída pro PDF417
    /// </summary>
    public class PDF417 : Barcode2D
    {
        int _columns = 6;
        int _errorCorrectionLevel = 3;
        private Hashtable _arrBinary;

        /// <summary>
        /// konstruktor
        /// </summary>
        public PDF417()
        {
            ModuleHeightToWidthRatio = 3;
            Initialize();
        }

        /// <summary>
        /// počet sloupců výsledného BC
        /// </summary>
        public int Columns
        {
            protected get
            {
                return _columns;
            }
            set
            {
                if (value >= 1 && value <= 30) _columns = value;
            }
        }

        /// <summary>
        /// úroveň odolnosti výsledného BC proti chybám
        /// </summary>
        public int ErrorCorrectionLevel
        {
            protected get
            {
                return _errorCorrectionLevel;
            }
            set
            {
                if (value >= 0 && value <= 8) _errorCorrectionLevel = value;
            }
        }

        /// <summary>
        /// zkontroluje vstupní text, vyhodí nepovolené znaky
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <returns></returns>
        protected override String CheckBarcodeText(String BarcodeText)
        {
            //TODO: dodat implementaci
            if (BarcodeText == "") return "EMPTY";
            return BarcodeText;
        }

        /// <summary>
        /// vrátí binární reprezentaci barcode, řádky jsou odděleny znakem "|"
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <returns></returns>
        protected override string GetBinary(string BarcodeText)
        {
            int EC;
            StringBuilder sb = new StringBuilder();
            try
            {
                string s = GetPDF417Text(BarcodeText, ErrorCorrectionLevel, Columns, out EC);
                for (int i = 0; i < s.Length; i++)
                {
                    sb.Append(_arrBinary[Encoding.GetEncoding(1250).GetBytes(s.Substring(i, 1))[0].ToString()]);
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                Exception = e;
                ExceptionUserMessage += "Chyba při převodu na binární reprezentaci\n";
                throw e;
            }
        }

        /// <summary>
        /// převede zadaný text na řetězec, který se dál zpracuje na binární data
        /// </summary>
        /// <param name="BarcodeText">text</param>
        /// <param name="ErrorCorrectionLevel">úroveň odolnosti proti chybám, max. 9</param>
        /// <param name="Columns">počet sloupců výsledného BC</param>
        /// <param name="ErrorCode">návratová hodnota - případný chybový stav</param>
        /// <returns></returns>
        private string GetPDF417Text(string BarcodeText, int ErrorCorrectionLevel, int Columns, out int ErrorCode)
        {
            int I = 0;
            int J = 0;
            int K = 0;
            int indexChaine = 0;
            string dummy;
            Boolean flag;
            TwoDimensionalArray liste = new TwoDimensionalArray();
            int indexListe = 0;
            int longueur;
            string chaineMC = "";
            Int64 total;
            TwoDimensionalArray listeT = new TwoDimensionalArray();
            int curTable;
            string chaineT;
            int newTable;
            OneDimensionalArray MCcorrection = new OneDimensionalArray();
            int c1;
            int c2;
            int c3;
            int mode = 0;
            int codeASCII = 0;
            string chaineMod;
            int diviseur;
            string chaineMult = "";
            int nombre = 0;
            string ASCII;
            string pdf417 = "";

            ASCII = "07260810082004151218042104100828082308241222042012131216121712190400040104020403040404050406040704080409121408000801042308020825080301000101010201030104010501060107010801090110011101120113011401150116011701180119012001210122012301240125080408050806042408070808020002010202020302040205020602070208020902100211021202130214021502160217021802190220022102220223022402250826082108270809";
            string[] coefRS = new string[9];
            coefRS[0] = "027917";
            coefRS[1] = "522568723809";
            coefRS[2] = "237308436284646653428379";
            coefRS[3] = "274562232755599524801132295116442428295042176065";
            coefRS[4] = "361575922525176586640321536742677742687284193517273494263147593800571320803133231390685330063410";
            coefRS[5] = "539422006093862771453106610287107505733877381612723476462172430609858822543376511400672762283184440035519031460594225535517352605158651201488502648733717083404097280771840629004381843623264543";
            coefRS[6] = "521310864547858580296379053779897444400925749415822093217208928244583620246148447631292908490704516258457907594723674292272096684432686606860569193219129186236287192775278173040379712463646776171491297763156732095270447090507048228821808898784663627378382262380602754336089614087432670616157374242726600269375898845454354130814587804034211330539297827865037517834315550086801004108539";
            coefRS[7] = "524894075766882857074204082586708250905786138720858194311913275190375850438733194280201280828757710814919089068569011204796605540913801700799137439418592668353859370694325240216257284549209884315070329793490274877162749812684461334376849521307291803712019358399908103511051008517225289470637731066255917269463830730433848585136538906090002290743199655903329049802580355588188462010134628320479130739071263318374601192605142673687234722384177752607640455193689707805641048060732621895544261852655309697755756060231773434421726528503118049795032144500238836394280566319009647550073914342126032681331792620060609441180791893754605383228749760213054297134054834299922191910532609829189020167029872449083402041656505579481173404251688095497555642543307159924558648055497010";
            coefRS[8] = "352077373504035599428207409574118498285380350492197265920155914299229643294871306088087193352781846075327520435543203666249346781621640268794534539781408390644102476499290632545037858916552041542289122272383800485098752472761107784860658741290204681407855085099062482180020297451593913142808684287536561076653899729567744390513192516258240518794395768848051610384168190826328596786303570381415641156237151429531207676710089168304402040708575162864229065861841512164477221092358785288357850836827736707094008494114521002499851543152729771095248361578323856797289051684466533820669045902452167342244173035463651051699591452578037124298332552043427119662777475850764364578911283711472420245288594394511327589777699688043408842383721521560644714559062145873663713159672729";
            coefRS[8] = coefRS[8] + "624059193417158209563564343693109608563365181772677310248353708410579870617841632860289536035777618586424833077597346269757632695751331247184045787680018066407369054492228613830922437519644905789420305441207300892827141537381662513056252341242797838837720224307631061087560310756665397808851309473795378031647915459806590731425216548249321881699535673782210815905303843922281073469791660162498308155422907817187062016425535336286437375273610296183923116667751353062366691379687842037357720742330005039923311424242749321054669316342299534105667488640672576540316486721610046656447171616464190531297321762752533175134014381433717045111020596284736138646411877669141919045780407164332899165726600325498655357752768223849647063310863251366304282738675410389244031121303263";
            string[] codageMC = new string[3];
            codageMC[0] = "urAxfsypyunkxdwyozpDAulspBkeBApAseAkprAuvsxhypnkutwxgzfDAplsfBkfrApvsuxyfnkptwuwzflspsyfvspxyftwpwzfxyyrxufkxFwymzonAudsxEyolkucwdBAoksucidAkokgdAcovkuhwxazdnAotsugydlkoswugjdksosidvkoxwuizdtsowydswowjdxwoyzdwydwjofAuFsxCyodkuEwxCjclAocsuEickkocgckcckEcvAohsuayctkogwuajcssogicsgcsacxsoiycwwoijcwicyyoFkuCwxBjcdAoEsuCicckoEguCbcccoEaccEoEDchkoawuDjcgsoaicggoabcgacgDobjcibcFAoCsuBicEkoCguBbcEcoCacEEoCDcECcascagcaacCkuAroBaoBDcCBtfkwpwyezmnAtdswoymlktcwwojFBAmksFAkmvkthwwqzFnAmtstgyFlkmswFksFkgFvkmxwtizFtsmwyFswFsiFxwmyzFwyFyzvfAxpsyuyvdkxowyujqlAvcsxoiqkkvcgxobqkcvcamfAtFswmyqvAmdktEwwmjqtkvgwxqjhlAEkkmcgtEbhkkqsghkcEvAmhstayhvAEtkmgwtajhtkqwwvijhssEsghsgExsmiyhxsEwwmijhwwqyjhwiEyyhyyEyjhyjvFkxmwytjqdAvEsxmiqckvEgxmbqccvEaqcEqcCmFktCwwljqhkmEstCigtAEckvaitCbgskEccmEagscqgamEDEcCEhkmawtDjgxkEgsmaigwsqiimabgwgEgaEgDEiwmbjgywEiigyiEibgybgzjqFAvCsxliqEkvCgxlbqEcvCaqEEvCDqECqEBEFAmCstBighAEEkmCgtBbggkqagvDbggcEEEmCDggEqaDgg";
            codageMC[0] = codageMC[0] + "CEasmDigisEagmDbgigqbbgiaEaDgiDgjigjbqCkvBgxkrqCcvBaqCEvBDqCCqCBECkmBgtArgakECcmBagacqDamBDgaEECCgaCECBEDggbggbagbDvAqvAnqBBmAqEBEgDEgDCgDBlfAspsweyldksowClAlcssoiCkklcgCkcCkECvAlhssqyCtklgwsqjCsslgiCsgCsaCxsliyCwwlijCwiCyyCyjtpkwuwyhjndAtoswuincktogwubncctoancEtoDlFksmwwdjnhklEssmiatACcktqismbaskngglEaascCcEasEChklawsnjaxkCgstrjawsniilabawgCgaawaCiwlbjaywCiiayiCibCjjazjvpAxusyxivokxugyxbvocxuavoExuDvoCnFAtmswtirhAnEkxviwtbrgkvqgxvbrgcnEEtmDrgEvqDnEBCFAlCssliahACEklCgslbixAagknagtnbiwkrigvrblCDiwcagEnaDiwECEBCaslDiaisCaglDbiysaignbbiygrjbCaDaiDCbiajiCbbiziajbvmkxtgywrvmcxtavmExtDvmCvmBnCktlgwsrraknCcxtrracvnatlDraEnCCraCnCBraBCCklBgskraakCCclBaiikaacnDalBDiicrbaCCCiiEaaCCCBaaBCDglBrabgCDaijgabaCDDijaabDCDrijrvlcxsqvlExsnvlCvlBnBctkqrDcnBEtknrDEvlnrDCnBBrDBCBclAqaDcCBElAnibcaDEnBnibErDnCBBibCaDBibBaDqibqibnxsfvkltkfnAmnAlCAoaBoiDoCAlaBlkpkBdAkosBckkogsebBcckoaBcEkoDBhkkqwsfjBgskqiBggkqbBgaBgDBiwkrjBiiBibBjjlpAsuswhil";
            codageMC[0] = codageMC[0] + "oksuglocsualoEsuDloCBFAkmssdiDhABEksvisdbDgklqgsvbDgcBEEkmDDgElqDBEBBaskniDisBagknbDiglrbDiaBaDBbiDjiBbbDjbtukwxgyirtucwxatuEwxDtuCtuBlmkstgnqklmcstanqctvastDnqElmCnqClmBnqBBCkklgDakBCcstrbikDaclnaklDbicnraBCCbiEDaCBCBDaBBDgklrDbgBDabjgDbaBDDbjaDbDBDrDbrbjrxxcyyqxxEyynxxCxxBttcwwqvvcxxqwwnvvExxnvvCttBvvBllcssqnncllEssnrrcnnEttnrrEvvnllBrrCnnBrrBBBckkqDDcBBEkknbbcDDEllnjjcbbEnnnBBBjjErrnDDBjjCBBqDDqBBnbbqDDnjjqbbnjjnxwoyyfxwmxwltsowwfvtoxwvvtmtslvtllkossfnlolkmrnonlmlklrnmnllrnlBAokkfDBolkvbDoDBmBAljbobDmDBljbmbDljblDBvjbvxwdvsuvstnkurlurltDAubBujDujDtApAAokkegAocAoEAoCAqsAqgAqaAqDAriArbkukkucshakuEshDkuCkuBAmkkdgBqkkvgkdaBqckvaBqEkvDBqCAmBBqBAngkdrBrgkvrBraAnDBrDAnrBrrsxcsxEsxCsxBktclvcsxqsgnlvEsxnlvCktBlvBAlcBncAlEkcnDrcBnEAlCDrEBnCAlBDrCBnBAlqBnqAlnDrqBnnDrnwyowymwylswotxowyvtxmswltxlksosgfltoswvnvoltmkslnvmltlnvlAkokcfBloksvDnoBlmAklbroDnmBllbrmDnlAkvBlvDnvbrvyzeyzdwyexyuwydxytswetwuswdvxutwtvxtkselsuksdntulstrvu";
            codageMC[1] = "ypkzewxdAyoszeixckyogzebxccyoaxcEyoDxcCxhkyqwzfjutAxgsyqiuskxggyqbuscxgausExgDusCuxkxiwyrjptAuwsxiipskuwgxibpscuwapsEuwDpsCpxkuywxjjftApwsuyifskpwguybfscpwafsEpwDfxkpywuzjfwspyifwgpybfwafywpzjfyifybxFAymszdixEkymgzdbxEcymaxEEymDxECxEBuhAxasyniugkxagynbugcxaaugExaDugCugBoxAuisxbiowkuigxbbowcuiaowEuiDowCowBdxAoysujidwkoygujbdwcoyadwEoyDdwCdysozidygozbdyadyDdzidzbxCkylgzcrxCcylaxCEylDxCCxCBuakxDgylruacxDauaExDDuaCuaBoikubgxDroicubaoiEubDoiCoiBcykojgubrcycojacyEojDcyCcyBczgojrczaczDczrxBcykqxBEyknxBCxBBuDcxBquDExBnuDCuDBobcuDqobEuDnobCobBcjcobqcjEobncjCcjBcjqcjnxAoykfxAmxAluBoxAvuBmuBloDouBvoDmoDlcbooDvcbmcblxAexAduAuuAtoBuoBtwpAyeszFiwokyegzFbwocyeawoEyeDwoCwoBthAwqsyfitgkwqgyfbtgcwqatgEwqDtgCtgBmxAtiswrimwktigwrbmwctiamwEtiDmwCmwBFxAmystjiFwkmygtjbFwcmyaFwEmyDFwCFysmziFygmzbFyaFyDFziFzbyukzhghjsyuczhahbwyuEzhDhDyyuCyuBwmkydgzErxqkwmczhrxqcyvaydDxqEwmCxqCwmBxqBtakwngydrviktacwnavicxrawnDviEtaCviCtaBviBmiktbgwnrqykmictb";
            codageMC[1] = codageMC[1] + "aqycvjatbDqyEmiCqyCmiBqyBEykmjgtbrhykEycmjahycqzamjDhyEEyChyCEyBEzgmjrhzgEzahzaEzDhzDEzrytczgqgrwytEzgngnyytCglzytBwlcycqxncwlEycnxnEytnxnCwlBxnBtDcwlqvbctDEwlnvbExnnvbCtDBvbBmbctDqqjcmbEtDnqjEvbnqjCmbBqjBEjcmbqgzcEjEmbngzEqjngzCEjBgzBEjqgzqEjngznysozgfgfyysmgdzyslwkoycfxloysvxlmwklxlltBowkvvDotBmvDmtBlvDlmDotBvqbovDvqbmmDlqblEbomDvgjoEbmgjmEblgjlEbvgjvysegFzysdwkexkuwkdxkttAuvButAtvBtmBuqDumBtqDtEDugbuEDtgbtysFwkFxkhtAhvAxmAxqBxwekyFgzCrwecyFaweEyFDweCweBsqkwfgyFrsqcwfasqEwfDsqCsqBliksrgwfrlicsraliEsrDliCliBCykljgsrrCycljaCyEljDCyCCyBCzgljrCzaCzDCzryhczaqarwyhEzananyyhCalzyhBwdcyEqwvcwdEyEnwvEyhnwvCwdBwvBsncwdqtrcsnEwdntrEwvntrCsnBtrBlbcsnqnjclbEsnnnjEtrnnjClbBnjBCjclbqazcCjElbnazEnjnazCCjBazBCjqazqCjnaznzioirsrfyziminwrdzzililyikzygozafafyyxozivivyadzyxmyglitzyxlwcoyEfwtowcmxvoyxvwclxvmwtlxvlslowcvtnoslmvrotnmsllvrmtnlvrllDoslvnbolDmrjonbmlDlrjmnblrjlCbolDvajoCbmizoajmCblizmajlizlCbvajvzieifwrFzzididyiczygeaFzywuy";
            codageMC[1] = codageMC[1] + "gdihzywtwcewsuwcdxtuwstxttskutlusktvnutltvntlBunDulBtrbunDtrbtCDuabuCDtijuabtijtziFiFyiEzygFywhwcFwshxsxskhtkxvlxlAxnBxrDxCBxaDxibxiCzwFcyCqwFEyCnwFCwFBsfcwFqsfEwFnsfCsfBkrcsfqkrEsfnkrCkrBBjckrqBjEkrnBjCBjBBjqBjnyaozDfDfyyamDdzyalwEoyCfwhowEmwhmwElwhlsdowEvsvosdmsvmsdlsvlknosdvlroknmlrmknllrlBboknvDjoBbmDjmBblDjlBbvDjvzbebfwnpzzbdbdybczyaeDFzyiuyadbhzyitwEewguwEdwxuwgtwxtscustuscttvustttvtklulnukltnrulntnrtBDuDbuBDtbjuDbtbjtjfsrpyjdwrozjcyjcjzbFbFyzjhjhybEzjgzyaFyihyyxwEFwghwwxxxxschssxttxvvxkkxllxnnxrrxBBxDDxbbxjFwrmzjEyjEjbCzjazjCyjCjjBjwCowCmwClsFowCvsFmsFlkfosFvkfmkflArokfvArmArlArvyDeBpzyDdwCewauwCdwatsEushusEtshtkdukvukdtkvtAnuBruAntBrtzDpDpyDozyDFybhwCFwahwixsEhsgxsxxkcxktxlvxAlxBnxDrxbpwnuzboybojDmzbqzjpsruyjowrujjoijobbmyjqybmjjqjjmwrtjjmijmbbljjnjjlijlbjkrsCusCtkFukFtAfuAftwDhsChsaxkExkhxAdxAvxBuzDuyDujbuwnxjbuibubDtjbvjjusrxijugrxbjuajuDbtijvibtbjvbjtgrwrjtajtDbsrjtrjsqjsnBxjDxiDxbbxgnyrbxabxDDwrbxrbwqbwn";
            codageMC[2] = "pjkurwejApbsunyebkpDwulzeDspByeBwzfcfjkprwzfEfbspnyzfCfDwplzzfBfByyrczfqfrwyrEzfnfnyyrCflzyrBxjcyrqxjEyrnxjCxjBuzcxjquzExjnuzCuzBpzcuzqpzEuznpzCdjAorsufydbkonwudzdDsolydBwokzdAyzdodrsovyzdmdnwotzzdldlydkzynozdvdvyynmdtzynlxboynvxbmxblujoxbvujmujlozoujvozmozlcrkofwuFzcnsodyclwoczckyckjzcucvwohzzctctycszylucxzyltxDuxDtubuubtojuojtcfsoFycdwoEzccyccjzchchycgzykxxBxuDxcFwoCzcEycEjcazcCycCjFjAmrstfyFbkmnwtdzFDsmlyFBwmkzFAyzFoFrsmvyzFmFnwmtzzFlFlyFkzyfozFvFvyyfmFtzyflwroyfvwrmwrltjowrvtjmtjlmzotjvmzmmzlqrkvfwxpzhbAqnsvdyhDkqlwvczhBsqkyhAwqkjhAiErkmfwtFzhrkEnsmdyhnsqtymczhlwEkyhkyEkjhkjzEuEvwmhzzhuzEthvwEtyzhthtyEszhszyduExzyvuydthxzyvtwnuxruwntxrttbuvjutbtvjtmjumjtgrAqfsvFygnkqdwvEzglsqcygkwqcjgkigkbEfsmFygvsEdwmEzgtwqgzgsyEcjgsjzEhEhyzgxgxyEgzgwzycxytxwlxxnxtDxvbxmbxgfkqFwvCzgdsqEygcwqEjgcigcbEFwmCzghwEEyggyEEjggjEazgizgFsqCygEwqCjgEigEbECygayECjgajgCwqBjgCigCbEBjgDjgBigBbCrklfwspzCnsldyClwlczCkyCkjzCuCvwlhzzCtCtyCszyFuCx";
            codageMC[2] = codageMC[2] + "zyFtwfuwftsrusrtljuljtarAnfstpyankndwtozalsncyakwncjakiakbCfslFyavsCdwlEzatwngzasyCcjasjzChChyzaxaxyCgzawzyExyhxwdxwvxsnxtrxlbxrfkvpwxuzinArdsvoyilkrcwvojiksrciikgrcbikaafknFwtmzivkadsnEyitsrgynEjiswaciisiacbisbCFwlCzahwCEyixwagyCEjiwyagjiwjCazaiziyzifArFsvmyidkrEwvmjicsrEiicgrEbicaicDaFsnCyihsaEwnCjigwrajigiaEbigbCCyaayCCjiiyaajiijiFkrCwvljiEsrCiiEgrCbiEaiEDaCwnBjiawaCiiaiaCbiabCBjaDjibjiCsrBiiCgrBbiCaiCDaBiiDiaBbiDbiBgrAriBaiBDaAriBriAqiAnBfskpyBdwkozBcyBcjBhyBgzyCxwFxsfxkrxDfklpwsuzDdsloyDcwlojDciDcbBFwkmzDhwBEyDgyBEjDgjBazDizbfAnpstuybdknowtujbcsnoibcgnobbcabcDDFslmybhsDEwlmjbgwDEibgiDEbbgbBCyDayBCjbiyDajbijrpkvuwxxjjdArosvuijckrogvubjccroajcEroDjcCbFknmwttjjhkbEsnmijgsrqinmbjggbEajgabEDjgDDCwlljbawDCijiwbaiDCbjiibabjibBBjDDjbbjjjjjFArmsvtijEkrmgvtbjEcrmajEErmDjECjEBbCsnlijasbCgnlbjagrnbjaabCDjaDDBibDiDBbjbibDbjbbjCkrlgvsrjCcrlajCErlDjCCjCBbBgnkrjDgbBajDabBDjDDDArbBrjDrjBcrkqjBErknjBCjBBbAqjBqbAnjBnjAorkfjAmjAlb";
            codageMC[2] = codageMC[2] + "AfjAvApwkezAoyAojAqzBpskuyBowkujBoiBobAmyBqyAmjBqjDpkluwsxjDosluiDoglubDoaDoDBmwktjDqwBmiDqiBmbDqbAljBnjDrjbpAnustxiboknugtxbbocnuaboEnuDboCboBDmsltibqsDmgltbbqgnvbbqaDmDbqDBliDniBlbbriDnbbrbrukvxgxyrrucvxaruEvxDruCruBbmkntgtwrjqkbmcntajqcrvantDjqEbmCjqCbmBjqBDlglsrbngDlajrgbnaDlDjrabnDjrDBkrDlrbnrjrrrtcvwqrtEvwnrtCrtBblcnsqjncblEnsnjnErtnjnCblBjnBDkqblqDknjnqblnjnnrsovwfrsmrslbkonsfjlobkmjlmbkljllDkfbkvjlvrsersdbkejkubkdjktAeyAejAuwkhjAuiAubAdjAvjBuskxiBugkxbBuaBuDAtiBviAtbBvbDuklxgsyrDuclxaDuElxDDuCDuBBtgkwrDvglxrDvaBtDDvDAsrBtrDvrnxctyqnxEtynnxCnxBDtclwqbvcnxqlwnbvEDtCbvCDtBbvBBsqDtqBsnbvqDtnbvnvyoxzfvymvylnwotyfrxonwmrxmnwlrxlDsolwfbtoDsmjvobtmDsljvmbtljvlBsfDsvbtvjvvvyevydnwerwunwdrwtDsebsuDsdjtubstjttvyFnwFrwhDsFbshjsxAhiAhbAxgkirAxaAxDAgrAxrBxckyqBxEkynBxCBxBAwqBxqAwnBxnlyoszflymlylBwokyfDxolyvDxmBwlDxlAwfBwvDxvtzetzdlyenyulydnytBweDwuBwdbxuDwtbxttzFlyFnyhBwFDwhbwxAiqAinAyokjfAymAylAifAyvkzekzdAyeByuAydBytszp";
            ErrorCode = 0;
            if (BarcodeText == "") { ErrorCode = 1; return ""; }
            indexChaine = 1;
            QuelMode(ref codeASCII, BarcodeText, indexChaine, ref mode);
            do
            {
                liste.Set(1, indexListe, mode);
                while (liste.Get(1, indexListe) == mode)
                {
                    liste.Set(0, indexListe, liste.Get(0, indexListe) + 1);
                    indexChaine = indexChaine + 1;
                    if (indexChaine > BarcodeText.Length) break;
                    QuelMode(ref codeASCII, BarcodeText, indexChaine, ref mode);
                }
                indexListe = indexListe + 1;
            }
            while (indexChaine <= BarcodeText.Length);
            for (int i = 0; i <= indexListe - 1; i++)
            {
                if (liste.Get(1, i) == 902)
                {
                    if (i == 0)
                    {
                        if (indexListe > 1)
                        {
                            if (liste.Get(1, i + 1) == 900)
                            {
                                if (liste.Get(0, i) < 8) { liste.Set(1, i, 900); }
                            }
                            else if (liste.Get(1, i + 1) == 901)
                            {
                                if (liste.Get(0, i) == 1) { liste.Set(1, i, 901); }
                            }
                        }
                    }
                    else
                    {
                        if (i == indexListe - 1)
                        {
                            if (liste.Get(1, i - 1) == 900)
                            {
                                if (liste.Get(0, i) < 7) { liste.Set(1, i, 900); }
                            }
                            else if (liste.Get(1, i - 1) == 901)
                            {
                                if (liste.Get(0, i) == 1) { liste.Set(1, i, 901); }
                            }
                        }
                        else
                        {
                            if (liste.Get(1, i - 1) == 901 && liste.Get(1, i + 1) == 901)
                            {
                                if (liste.Get(0, i) < 4) { liste.Set(1, i, 901); }
                            }
                            else if (liste.Get(1, i - 1) == 900 && liste.Get(1, i + 1) == 901)
                            {
                                if (liste.Get(0, i) < 5) { liste.Set(1, i, 900); }
                            }
                            else if (liste.Get(1, i - 1) == 900 && liste.Get(1, i + 1) == 900)
                            {
                                if (liste.Get(0, i) < 8) { liste.Set(1, i, 900); }
                            }
                        }
                    }
                }
            }

            Regroupe(ref indexListe, ref liste, ref I, ref J);
            for (int i = 0; i <= indexListe - 1; i++)
            {
                if (liste.Get(1, i) == 900 && i > 0)
                {
                    if (i == indexListe - 1)
                    {
                        if (liste.Get(1, i - 1) == 901)
                        {
                            if (liste.Get(0, i) == 1) { liste.Set(1, i, 901); }
                        }
                        else
                        {
                            if (liste.Get(1, i - 1) == 901 && liste.Get(1, i + 1) == 901)
                            {
                                if (liste.Get(0, i) < 5) { liste.Set(1, i, 901); }
                                else if ((liste.Get(1, i - 1) == 901 && liste.Get(1, i + 1) != 901) || (liste.Get(1, i - 1) != 901 && liste.Get(1, i + 1) == 901))
                                {
                                    if (liste.Get(0, i) < 3) { liste.Set(1, i, 901); }
                                }
                            }
                        }
                    }
                }
            }
            Regroupe(ref indexListe, ref liste, ref I, ref J);
            indexChaine = 1;
            for (int ii = 0; ii <= indexListe - 1; ii++)
            {
                switch (liste.Get(1, ii))
                {
                    case 900:
                        listeT = new TwoDimensionalArray();
                        for (int indexListeT = 0; indexListeT <= liste.Get(0, ii) - 1; indexListeT++)
                        {
                            codeASCII = Encoding.GetEncoding(1250).GetBytes(BarcodeText.Substring(indexChaine + indexListeT - 1, 1))[0];
                            switch (codeASCII)
                            {
                                case 9:
                                    {
                                        listeT.Set(0, indexListeT, 12);
                                        listeT.Set(1, indexListeT, 12);
                                        break;
                                    }
                                case 10:
                                    {
                                        listeT.Set(0, indexListeT, 8);
                                        listeT.Set(1, indexListeT, 15);
                                        break;
                                    }
                                case 13:
                                    {
                                        listeT.Set(0, indexListeT, 12);
                                        listeT.Set(1, indexListeT, 11);
                                        break;
                                    }
                                default:
                                    {
                                        listeT.Set(0, indexListeT, int.Parse(ASCII.Substring(codeASCII * 4 - 127 - 1, 2)));
                                        listeT.Set(1, indexListeT, int.Parse(ASCII.Substring(codeASCII * 4 - 125 - 1, 2)));
                                    }
                                    break;
                            }
                        }
                        curTable = 1;
                        chaineT = "";
                        for (int j = 0; j <= liste.Get(0, ii) - 1; j++)
                        {
                            if ((listeT.Get(0, j) & curTable) > 0)
                            {
                                chaineT = chaineT + listeT.Get(1, j).ToString("00");
                            }
                            else
                            {
                                flag = false;
                                if (j == liste.Get(0, ii) - 1)
                                {
                                    flag = true;
                                }
                                else
                                {
                                    if ((listeT.Get(0, j) & listeT.Get(0, j + 1)) == 0) flag = true;
                                }
                                if (flag)
                                {
                                    if ((listeT.Get(0, j) & 1) > 0 && curTable == 2)
                                    {
                                        chaineT = chaineT + "27" + listeT.Get(1, j).ToString("00");
                                    }
                                    else if ((listeT.Get(0, j) & 8) > 0)
                                    {
                                        chaineT = chaineT + "29" + listeT.Get(1, j).ToString("00");
                                    }
                                    else
                                    {
                                        flag = false;
                                    }
                                }

                                if (!flag)
                                {
                                    if (j == liste.Get(0, ii) - 1)
                                    {
                                        newTable = listeT.Get(0, j);
                                    }
                                    else
                                    {
                                        newTable = ((listeT.Get(0, j) & listeT.Get(0, j + 1)) == 0) ? listeT.Get(0, j) : listeT.Get(0, j) & listeT.Get(0, j + 1);
                                    }

                                    if (newTable == 3 || newTable == 5 || newTable == 7 || newTable == 9 || newTable == 11 || newTable == 13 || newTable == 15) newTable = 1;
                                    if (newTable == 6 || newTable == 10 || newTable == 14) newTable = 2;
                                    if (newTable == 12) newTable = 4;

                                    switch (curTable)
                                    {
                                        case 1:
                                            switch (newTable)
                                            {
                                                case 2:
                                                    chaineT = chaineT + "27";
                                                    break;
                                                case 4:
                                                    chaineT = chaineT + "28";
                                                    break;
                                                case 8:
                                                    chaineT = chaineT + "2825";
                                                    break;
                                            }
                                            break;
                                        case 2:
                                            switch (newTable)
                                            {
                                                case 1:
                                                    chaineT = chaineT + "2828";
                                                    break;
                                                case 4:
                                                    chaineT = chaineT + "28";
                                                    break;
                                                case 8:
                                                    chaineT = chaineT + "2825";
                                                    break;
                                            }
                                            break;
                                        case 4:
                                            switch (newTable)
                                            {
                                                case 1:
                                                    chaineT = chaineT + "28";
                                                    break;
                                                case 2:
                                                    chaineT = chaineT + "27";
                                                    break;
                                                case 8:
                                                    chaineT = chaineT + "25";
                                                    break;
                                            }
                                            break;
                                        case 8:
                                            switch (newTable)
                                            {
                                                case 1:
                                                    chaineT = chaineT + "29";
                                                    break;
                                                case 2:
                                                    chaineT = chaineT + "2927";
                                                    break;
                                                case 4:
                                                    chaineT = chaineT + "2928";
                                                    break;
                                            }
                                            break;
                                    }
                                    curTable = newTable;
                                    chaineT = chaineT + listeT.Get(1, j).ToString("00");
                                }
                            }
                        }
                        if (chaineT.Length % 4 > 0) chaineT = chaineT + "29";
                        if (ii > 0) chaineMC = chaineMC + "900";
                        for (int j = 1; j <= chaineT.Length; j += 4)
                        {
                            chaineMC = chaineMC + (int.Parse(chaineT.Substring(j - 1, 2)) * 30 + int.Parse(chaineT.Substring(j + 2 - 1, 2))).ToString("000");
                        }
                        break;
                    case 901:
                        if (liste.Get(0, ii) == 1)
                        {
                            chaineMC = chaineMC + "913" + (Encoding.GetEncoding(1250).GetBytes(BarcodeText.Substring(indexChaine - 1, 1))[0]).ToString("000");
                        }
                        else
                        {
                            if (liste.Get(0, ii) % 6 == 0)
                            {
                                chaineMC = chaineMC + "924";
                            }
                            else
                            {
                                chaineMC = chaineMC + "901";
                            }
                            J = 0;
                            while (J < liste.Get(0, ii))
                            {
                                longueur = liste.Get(0, ii) - J;
                                if (longueur >= 6)
                                {
                                    longueur = 6;
                                    total = 0;
                                    for (int k = 0; k <= longueur - 1; k++)
                                    {
                                        total = total + (Encoding.GetEncoding(1250).GetBytes(BarcodeText.Substring(indexChaine + J + k - 1, 1))[0] * (Int64)Math.Pow(256, (longueur - 1 - k)));
                                    }
                                    chaineMod = String.Format(total.ToString(), "general number");
                                    dummy = "";
                                    do
                                    {
                                        diviseur = 900;
                                        Modulo(ref chaineMult, ref nombre, ref chaineMod, ref diviseur);
                                        dummy = diviseur.ToString("000") + dummy;
                                        chaineMod = chaineMult;
                                    }
                                    while (chaineMult != "");
                                    chaineMC = chaineMC + dummy;
                                }
                                else
                                {
                                    for (int k = 0; k <= longueur - 1; k++)
                                    {
                                        chaineMC = chaineMC + (Encoding.GetEncoding(1250).GetBytes(BarcodeText.Substring(indexChaine + J + k - 1, 1))[0]).ToString("000");
                                    }
                                }
                                J = J + longueur;
                            }
                        }
                        break;
                    case 902:
                        chaineMC = chaineMC + "902";
                        J = 0;
                        while (J < liste.Get(0, ii))
                        {
                            longueur = liste.Get(0, ii) - J;
                            if (longueur > 44) longueur = 44;
                            chaineMod = "1" + BarcodeText.Substring(indexChaine + J - 1, longueur);
                            dummy = "";
                            do
                            {
                                diviseur = 900;
                                Modulo(ref chaineMult, ref nombre, ref chaineMod, ref diviseur);
                                dummy = diviseur.ToString("000") + dummy;
                                chaineMod = chaineMult;
                            }
                            while (chaineMult != "");
                            chaineMC = chaineMC + dummy;
                            J = J + longueur;
                        }
                        break;
                }
                indexChaine = indexChaine + liste.Get(0, ii);
            }
            longueur = chaineMC.Length / 3;
            if (ErrorCorrectionLevel < 0)
            {
                if (longueur < 41)
                {
                    ErrorCorrectionLevel = 2;
                }
                else if (longueur < 161)
                {
                    ErrorCorrectionLevel = 3;
                }
                else if (longueur < 321)
                {
                    ErrorCorrectionLevel = 4;
                }
                else
                {
                    ErrorCorrectionLevel = 5;
                }
            }
            longueur = longueur + 1 + (int)Math.Pow(2, (ErrorCorrectionLevel + 1));
            if (Columns > 30) Columns = 30;
            if (Columns < 1)
            {
                Columns = (int)((Math.Sqrt(204 * longueur + 4761) - 69) / (34 / 1.3));
                if (Columns == 0) Columns = 1;
            }
            while (ErrorCorrectionLevel > 0)
            {
                longueur = chaineMC.Length / 3 + 1 + (int)Math.Pow(2, (ErrorCorrectionLevel + 1));
                longueur = ((int)(longueur / Columns) + (longueur % Columns > 0 ? 1 : 0)) * Columns;
                if (longueur < 929) break;
                ErrorCorrectionLevel = ErrorCorrectionLevel - 1;
                ErrorCode = 10;
            }
            if (longueur > 928)
            {
                ErrorCode = 2;
                return "";
            }
            if (longueur / Columns > 90)
            {
                ErrorCode = 3;
                return "";
            }
            longueur = chaineMC.Length / 3 + 1 + (int)Math.Pow(2, (ErrorCorrectionLevel + 1));
            I = 0;
            if ((int)(longueur / Columns) < 3)
            {
                I = Columns * 3 - longueur;
            }
            else
            {
                if (longueur % Columns > 0) I = Columns - (longueur % Columns);
            }
            while (I > 0)
            {
                chaineMC = chaineMC + "900";
                I = I - 1;
            }
            chaineMC = (chaineMC.Length / 3 + 1).ToString("000") + chaineMC;
            longueur = chaineMC.Length / 3;
            K = (int)Math.Pow(2, (ErrorCorrectionLevel + 1));
            MCcorrection = new OneDimensionalArray();
            total = 0;
            for (int iii = 0; iii <= longueur - 1; iii++)
            {

                total = (int.Parse(chaineMC.Substring(iii * 3 + 1 - 1, 3)) + MCcorrection.Get(K - 1)) % 929;
                for (int j = K - 1; j >= 0; j--)
                {
                    if (j == 0)
                    {
                        MCcorrection.Set(j, (929 - (total * Int64.Parse(coefRS[ErrorCorrectionLevel].Substring(j * 3 + 1 - 1, 3))) % 929) % 929);
                    }
                    else
                    {
                        MCcorrection.Set(j, (MCcorrection.Get(j - 1) + 929 - (total * int.Parse(coefRS[ErrorCorrectionLevel].Substring(j * 3 + 1 - 1, 3))) % 929) % 929);
                    }
                }
            }
            for (int j = 0; j <= K - 1; j++)
            {
                if (MCcorrection.Get(j) != 0) MCcorrection.Set(j, 929 - MCcorrection.Get(j));
            }

            for (int ii = K - 1; ii >= 0; ii--)
            {
                chaineMC = chaineMC + MCcorrection.Get(ii).ToString("000");
            }
            c1 = (int)((chaineMC.Length / 3 / Columns - 1) / 3);
            c2 = ErrorCorrectionLevel * 3 + (chaineMC.Length / 3 / Columns - 1) % 3;
            c3 = Columns - 1;
            for (int ii = 0; ii <= chaineMC.Length / 3 / Columns - 1; ii++)
            {
                dummy = chaineMC.Substring(ii * Columns * 3 + 1 - 1, Columns * 3);
                K = (int)(ii / 3) * 30;
                switch (ii % 3)
                {
                    case 0:
                        dummy = (K + c1).ToString("000") + dummy + (K + c3).ToString("000");
                        break;
                    case 1:
                        dummy = (K + c2).ToString("000") + dummy + (K + c1).ToString("000");
                        break;
                    case 2:
                        dummy = (K + c3).ToString("000") + dummy + (K + c2).ToString("000");
                        break;
                }
                pdf417 = pdf417 + "+*";
                for (int j = 0; j <= dummy.Length / 3 - 1; j++)
                {
                    pdf417 = pdf417 + codageMC[ii % 3].Substring(int.Parse(dummy.Substring(j * 3 + 1 - 1, 3)) * 3 + 1 - 1, 3) + "*";
                }
                pdf417 = pdf417 + "-" + "|";
            }
            return pdf417;
        }



        /// <summary>
        /// pomocná funkce pro GetPDF417Text
        /// </summary>
        /// <param name="indexListe"></param>
        /// <param name="liste"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void Regroupe(ref int indexListe, ref TwoDimensionalArray liste, ref int i, ref int j)
        {
            if (indexListe > 1)
            {
                i = 1;
                while (i < indexListe)
                {
                    if (liste.Get(1, i - 1) == liste.Get(1, i))
                    {
                        liste.Set(0, i - 1, liste.Get(0, i - 1) + liste.Get(0, i));
                        j = i + 1;
                        while (j < indexListe)
                        {
                            liste.Set(0, j - 1, liste.Get(0, j));
                            liste.Set(1, j - 1, liste.Get(1, j));
                            j += 1;
                        }
                        indexListe -= 1;
                        i -= 1;
                    }
                    i += 1;
                }
            }
        }


        /// <summary>
        /// pomocná funkce pro GetPDF417Text
        /// </summary>
        /// <param name="codeASCII"></param>
        /// <param name="chaine"></param>
        /// <param name="indexChaine"></param>
        /// <param name="mode"></param>
        private void QuelMode(ref int codeASCII, string chaine, int indexChaine, ref int mode)
        {
            //codeASCII = (int)chaine.Substring(indexChaine - 1, 1)[0];
            codeASCII = Encoding.GetEncoding(1250).GetBytes(chaine.Substring(indexChaine - 1, 1))[0];
            if (codeASCII >= 48 && codeASCII <= 57) { mode = 902; return; }
            if (codeASCII == 9 || codeASCII == 10 || codeASCII == 13 || (codeASCII >= 32 && codeASCII <= 126)) { mode = 900; return; }
            mode = 901;
            return;
        }


        /// <summary>
        /// pomocná funkce pro GetPDF417Text
        /// </summary>
        /// <param name="ChaineMult"></param>
        /// <param name="Nombre"></param>
        /// <param name="ChaineMod"></param>
        /// <param name="Diviseur"></param>
        private void Modulo(ref string ChaineMult, ref int Nombre, ref string ChaineMod, ref int Diviseur)
        {
            ChaineMult = "";
            Nombre = 0;
            while (!ChaineMod.Equals(""))
            {
                Nombre = Nombre * 10 + int.Parse((ChaineMod.Substring(0, 1)));
                ChaineMod = ChaineMod.Substring(2 - 1);
                if (Nombre < Diviseur)
                {
                    if (!ChaineMult.Equals("")) ChaineMult = ChaineMult + "0";
                }
                else
                {
                    ChaineMult = ChaineMult + ((int)(Nombre / Diviseur)).ToString(); //celočíselné dělení !!!!!
                }
                Nombre = Nombre % Diviseur;
            }
            Diviseur = Nombre;
        }

        /// <summary>
        /// třída, která s pomocí hashtable nahrazuje dvourozměrné pole, protože C# neumí Redim Preserve ...
        /// </summary>
        private class TwoDimensionalArray
        {
            Hashtable _array;
            public TwoDimensionalArray()
            {
                _array = new Hashtable();
            }

            public int Get(object x, object y)
            {
                try
                {
                    return (int)_array[x.ToString() + "_" + y.ToString()];
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            public void Set(object x, object y, int Value)
            {
                try
                {
                    _array[x.ToString() + "_" + y.ToString()] = Value;

                }
                catch (Exception)
                {
                }
            }

            public Hashtable GetHT
            {
                get
                {
                    return _array;
                }
            }
        }

        /// <summary>
        /// třída, která s pomocí hashtable nahrazuje jednorozměrné pole, protože C# neumí Redim Preserve ...
        /// </summary>
        private class OneDimensionalArray
        {
            Hashtable _array;
            public OneDimensionalArray()
            {
                _array = new Hashtable();
            }

            public Int64 Get(object x)
            {
                try
                {
                    return (Int64)_array[x.ToString()];
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            public void Set(object x, Int64 Value)
            {
                try
                {
                    _array[x.ToString()] = Value;

                }
                catch (Exception)
                {
                }
            }

            public Hashtable GetHT
            {
                get
                {
                    return _array;
                }
            }
        }


        /// <summary>
        /// inicializuje tabulku pro převod na binární data
        /// </summary>
        private void Initialize()
        {
            _arrBinary = new Hashtable();
            _arrBinary["124"] = "|";
            _arrBinary["65"] = "00000";
            _arrBinary["66"] = "00001";
            _arrBinary["67"] = "00010";
            _arrBinary["68"] = "00011";
            _arrBinary["69"] = "00100";
            _arrBinary["70"] = "00101";
            _arrBinary["97"] = "00110";
            _arrBinary["98"] = "00111";
            _arrBinary["99"] = "01000";
            _arrBinary["100"] = "01001";
            _arrBinary["101"] = "01010";
            _arrBinary["102"] = "01011";
            _arrBinary["103"] = "01100";
            _arrBinary["104"] = "01101";
            _arrBinary["105"] = "01110";
            _arrBinary["106"] = "01111";
            _arrBinary["107"] = "10000";
            _arrBinary["108"] = "10001";
            _arrBinary["109"] = "10010";
            _arrBinary["110"] = "10011";
            _arrBinary["111"] = "10100";
            _arrBinary["112"] = "10101";
            _arrBinary["113"] = "10110";
            _arrBinary["115"] = "11000";
            _arrBinary["114"] = "10111";
            _arrBinary["116"] = "11001";
            _arrBinary["117"] = "11010";
            _arrBinary["118"] = "11011";
            _arrBinary["119"] = "11100";
            _arrBinary["120"] = "11101";
            _arrBinary["121"] = "11110";
            _arrBinary["122"] = "11111";
            _arrBinary["42"] = "01";
            _arrBinary["43"] = "1111111101010100";
            _arrBinary["45"] = "11111101000101001";
        }
    }
}
