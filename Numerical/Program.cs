﻿using CsharpALG.Numerical;
test_linspace();
test_bspline();
testLUFullPivot();
testSolve();
testLinearRegress();
testBsplineRegression();


void test_linspace()
{
    // foreach(var x in Utility.linspace(1.0, 17.0, 17))
    // {
    //     Console.Write($"{x} ");
    // }
    // Console.WriteLine();
    double[] elems = Utility.linspace(1.0, 17.0, 17).ToArray();
    Console.WriteLine($"{Utility.ArrToString(elems)}");
}

void test_bspline()
{
    _test_bspline(-5.0, 5.0, 8, 64, 4);
}

void testLUFullPivot()
{
    Console.WriteLine("testLUFullPivot");
    double[,] A = new double[6, 6]{
        { 0.6507243 ,  0.60397494, -0.166312  ,  0.60373338,  0.69549304,
         1.27848531},
       { 0.31783923, -0.26187952,  0.29240419, -0.42975706, -0.16683741,
        -0.78417863},
       { 1.43250853, -0.91312242,  0.22364118, -2.56178452, -2.64469449,
         0.41898378},
       {-0.27835331,  0.41572357, -1.00932634, -0.21634919, -0.9156908 ,
         0.15606333},
       { 1.20823136, -1.13378592,  0.72136082, -0.38756887, -0.39205936,
         0.19640329},
       { 1.53353118,  1.24952624, -0.53950851,  0.86485211, -1.14853424,
         2.32183358}
    };
    _testLUFullPivot(A);

    // rank == 5
    A = new double[10, 10]{
{80.43604778060805,-50.74807485621817,-225.86773058335854,-160.72091318464717,385.0149183306574,-149.02621736121492,-335.4151957259513,-344.6009404913548,-335.2605482267729,-122.66304872447091},
{334.80201791850584,-253.23719505622506,-222.69017821155734,-262.2550090825297,-116.22047117928433,-30.482101503828765,-669.7474839735934,242.76358965504755,-559.2615224740191,-609.0053239353512},
{-60.199737046363964,491.3567341396999,-307.08360733457124,661.5757814054326,107.94038496816506,300.4979730373869,52.84534971278555,-65.96509559930506,-59.99396842229325,424.22941331917986},
{421.50594798245527,-548.1914089681786,95.79714738289992,-476.73254845750586,-63.768464037187194,491.62552370834453,-406.75740216355524,-617.4927360200998,413.09438491976925,-513.3955634933454},
{192.9001800291231,-557.2709288788799,1061.9947220695085,-1538.668877322873,79.50192513670098,-595.3592368785505,143.51161822606758,477.8251177867155,1033.020837522371,-1140.1699490668184},
{166.9216443791255,-541.0383113483344,483.4644694273961,-624.5276990651289,-499.96433991714036,-31.671109328068205,17.271566126679506,376.6273428999645,368.9579016449749,-569.7444108880079},
{-107.45885540330619,-503.9267837495024,477.2646794568028,-629.8648383048434,169.95694050796243,-89.1527878560795,381.7406518727695,-732.8662508091336,712.6678189799943,34.42709305692899},
{2.218689259029518,-572.5495173803572,520.3551293227234,-988.0108980528836,-1.1596306098385136,-589.8691107162923,50.10099517720544,163.16114490750013,177.49387335919405,-565.8479815796513},
{138.3569019125175,-252.6931710749851,171.98205659727023,-494.19277946057855,238.54104538661136,-50.79745875679775,-132.3838814991938,-334.04170148148387,275.9874441604167,-325.06109224023197},
{-65.72290757480607,-366.70182850449464,365.7587873697493,-360.76016626378396,284.72928837546993,284.535622332348,409.0339682017255,-1144.4463162872955,962.3043230323658,256.96699682200665},
    };

    _testLUFullPivot(A);
}

void testSolve()
{
    Console.WriteLine("testSolve");
    double[,] A = new double[10, 10]
    {
{-365.3575287196817,-740.215844778857,5.437756471458902,649.0748963049565,472.93106383288006,-26.812662827336872,295.58330635180755,659.1841993555499,-541.5326141830009,-595.7139429192065},
{-936.8463945715527,-1719.4237306795485,73.9324626388504,411.3581441555095,317.2895586064727,143.89658373641117,-181.91103020648526,908.6028025593655,-650.5414196541362,-1576.3057494996315},
{-133.44177274945088,-1166.0469091169969,-86.74831801158942,-6.910513430265022,616.0452479287685,82.14021932695458,-608.558015399083,1040.3442631275664,660.3243523376633,-538.2540139440101},
{279.0827299988821,1082.6860599177032,-546.5834900948549,-1316.8533298852094,755.3733493033726,-261.6445876260986,655.1293617664129,-556.7667549762464,1220.125378082426,653.5038245873601},
{-64.90166311419655,105.28260739489251,-310.77918353238533,-1246.5654959431834,312.56422647416684,-145.32477715957447,-150.34024957753545,-572.8999895356592,1073.5886843862324,-162.58454219049227},
{509.47976216146543,248.52519031457715,-58.22193661814745,-650.3492277363797,-138.33698266327664,160.09987097819715,-750.5697082857147,195.93216944231906,1120.2142977773394,641.7652311766984},
{-129.8047653930199,-527.6904040503206,-175.46016140555477,632.2526592403299,996.4057607887845,-45.20569233565806,544.7408379917259,1167.1181803528104,-146.08224813599668,-177.32442542905827},
{-623.2613903257693,-389.57340629616914,-310.97557500482344,413.2454095636984,1171.8425694074726,-229.76146803956223,1310.5006725124658,562.4897078493752,-668.9351687645667,-702.2873540901888},
{-7.209300505910179,47.99138429404434,-46.5660188513372,42.530993094201975,184.83068332620937,-192.35268999691465,195.02083372887975,-352.5100574252458,-3.262158426652566,-70.70095232345535},
{-210.38446477710391,-713.1033514976025,182.57546670074265,869.8473683225714,93.12899545420404,-108.9798135909228,-17.435924552879023,163.782422760808,-640.7946926345497,-530.2632706502833}
    };
    double[] b = [-0.7164010059500501, 1.0365736233420006, 1.7936691055622056, 0.2554702678790001, -2.374315063951439, 2.2099837794573367, 0.18284154681837053, 1.720496618632065, -1.4795927483487459, 0.35323914751918783];
    _testSolve(A, b);
}

void testLinearRegress()
{
    Console.WriteLine("testLinearRegress");
    double[,] A = new double[10, 5]
    {
{-9.924598359639422,16.762251199166382,-4.15868622348162,-10.059641080758038,2.4650005591351647},
{13.897750260809818,3.9856572188394908,6.491989313325004,-3.4126890928420703,13.15534687017512},
{-5.966819145767757,-12.462310918100883,1.9781173121052902,3.531644708963495,-4.680275704503538},
{7.7858110404527405,-7.486517266492958,20.747157205558672,6.366130077407909,-5.83414602170932},
{-23.279019661279595,-10.058822573431794,-27.575225076968213,-8.394267763769253,11.604237461830252},
{10.875599208812972,0.682148759714623,19.022935413147657,-12.313541745251012,-8.838447101426926},
{5.255593756046725,3.247438960013784,4.339622187340969,-9.56210279725698,11.367338461310258},
{5.3987367032070654,-16.778426548214778,13.39068729443309,6.739591726468547,7.578467178899532},
{-10.184684891784034,12.497832615668155,-8.942139657537126,9.528777207850036,17.845134928647827},
{-7.083731070839769,-11.428012733447087,16.61652079345496,2.4347559162146664,1.695905339804741}
    };
    double[] b = [5.846383645010471, 3.559116185674854, 10.705722161880281, 7.277897804937165, 10.989067945778476, 5.208129453044686, 2.0987747283365494, 9.436732642315171, 0.3271123221655112, 10.822334965850198];
    _testLinearRegress(A, b, 3.0);
}

void testBsplineRegression()
{
    //  y = exp(sin(pi * x))
    double[] x = [-1.5871263948084593, 3.604477303634397, 2.8694493794778158, -0.5188068678626028, -0.28972292666750477, -5.040386240349049, -4.546515850682221, -4.055404552636224, -3.3127126839973453, -4.378164194021748, -1.6259569774529137, 4.395940880900898, 0.03206112284782314, 2.6264757625615918, -2.731825816591293, 2.9248579955884395, 2.9888147620035355, -1.7811335214650974, 2.5521829691848197, -5.5368159007969675, 4.2665485419917, -4.038455415852315, 3.125837714853258, 1.2796516835407346, 3.1471409436135804, -2.3996217887770035, -4.743000118832508, 1.7525438477628814, -0.570663660671662, 1.4118712477203639, -2.958577893512544, 1.3170577164734585, 3.524579302941955, -1.9313059681830769, -2.185630665608602, -1.1156745067206062, 1.3334466414149961, -5.921241830346762, -3.583278782694545, 0.06663548757885884, 2.489302436690302, 4.161558377297073, -3.4445360349520926, -1.1570548372382756, -2.456836068578954, 3.506144731329144, -4.429856944737345, -1.0063598477359035, -2.472510043310903, -2.226263170219089, 4.029658307901613, -5.241598943131091, 0.5545828938858284, 5.4285418946506745, -2.6404367208613606, -5.521857561742847, 1.2668682440840975, -0.334923398846513, -2.4837600508061852, 0.6398327851050265, 0.2616190223538055, 3.705743229616603, -1.2415568491797702, 0.0410578809181974, -5.607049470418217, 4.293413587757158, 5.6237717826593645, 5.458383435385661, 4.825723188524972, 4.5321920397655315, -0.9166923112612384, 0.38336615926595385, 4.88816369005186, -4.138800676937516, -0.6387584524837688, 3.2400716934833884, -0.23995791380187104, -4.682705675641362, 5.041071862536022, -4.8933731804837075, -4.599663099316928, 1.5932930731404795, -0.7570942148940638, -5.068393022692579, 3.3146977507637665, -4.503718295969701, -2.902673407979065, -1.138435309318103, 2.623544534640317, 3.309425351179865, -0.12321303674535855, 5.118155773101165, -2.197115573617013, -3.0426435081180063, -1.3596735705173657, 5.858283555507645, -4.101418070922335, 3.139384256151489, 3.5838458169171794, -2.426516862493724, -1.1091297331311116, 0.9475917810042249, -4.387457680835318, 1.1088514669772378, 2.843829426244758, 1.681337989428462, -4.879201376721793, -1.4853040643679538, -0.003955308065334506, -5.373215011544295, -4.915420721212952, 3.897593366031529, -1.5075094219769287, -5.26544363616333, 4.1101105970462655, 2.9435824192956623, -5.141412435129966, 0.7171594258081448, -4.590890387650628, 4.767466268106496, -0.5732317570409071, -3.188978232384798, -1.7070941384950924, -3.6619210238492452, -5.148341222928916, -5.369293184980139, 0.8025187666810121, -5.801949261448051, -3.070121846932263, -2.640260395636409, -2.384885778372137, -0.6162535631122878, -2.611621460375959, -4.526868279439896, 5.329069289133535, 4.348649735685523, -0.6736002146926285, 3.784706170056843, 2.529764467311308, 1.9253540357478531];
    double[] y = [2.713460200101604, 1.0246223518380393, 1.9543914860550486, 2.11354044763235, 1.297783271940804, 0.3843323158588847, 2.3618606090049354, 2.679972381363056, 1.0847896141363522, 0.6038097612882896, 2.4590309525690652, 0.426781606877729, 1.0032345062421046, 1.3687095732537213, 0.3703876021423379, 2.6784016797947974, 1.2323141899746233, 0.5971330614335971, 2.71578841636957, 2.4152487610880224, 1.8157251961412462, 2.2830842807912743, 0.517223873774761, 0.4032768080080999, 0.7441200006447556, 0.5022319850595075, 2.718072557892007, 0.8005439349760421, 2.3483590800853684, 0.979420190715801, 2.0137534263910104, 0.4769280638195485, 2.639598893005184, 0.4722579145024634, 1.905419402007971, 0.4989234917991822, 0.5262072547291607, 0.8263064460960463, 1.6194153484569813, 1.0140468673823213, 1.7845720161772454, 0.4308871528814688, 0.6623207268094277, 0.4170923762062965, 1.1196230540053347, 2.2166412634119905, 0.39632725872987856, 0.9607160028286172, 1.4169282283612248, 1.1468504999976104, 1.9744498683624063, 0.3690742498360508, 2.276755899501854, 0.36961924231761745, 1.0921466029293598, 2.7171739684229266, 0.3882402260246437, 1.4122089137323997, 1.6594863937096442, 2.611053387609295, 1.2378445940171887, 0.474817325160255, 0.3710093528708751, 1.0053099618973393, 0.37467511798021813, 2.659641336760894, 0.3978462307590772, 0.5471372828469605, 0.45588270381496, 2.696112031797088, 1.6174636389185664, 1.561251049040572, 0.7214734603087558, 0.6728726279004911, 2.6078675485390552, 2.718231581496102, 1.197106462884514, 0.798444499245645, 0.38198473629691165, 0.8423023385869874, 0.6230064526196948, 2.698410447954599, 2.647671164625543, 0.4362538427340225, 1.0409513634246417, 2.175879311966489, 2.645195097688159, 0.44859597208877416, 1.432450564856387, 1.161027842789846, 1.0488306320499972, 1.7795085507779713, 1.6757800693519525, 0.48482903969187374, 0.6328059204762124, 2.324400018525949, 1.7014752097325, 0.6454105813544064, 1.6013390568903776, 0.708440034264894, 0.5159685452331936, 1.3705261021623936, 0.4933044301188251, 0.5167198237316935, 1.311320532839383, 1.6776713858094379, 0.5650337202958575, 1.828051627220426, 1.0000491497343371, 1.4814084446195697, 1.6249803503501135, 0.5681978281558303, 2.1285821378405663, 0.46737809761843924, 1.3907294130751622, 2.3848425673027682, 2.660887259367905, 2.7155338432249763, 0.7887254670159293, 2.122795940148185, 2.3595860570587086, 1.6618162220522386, 1.3052388494603122, 0.38289286727850375, 2.717888191630189, 1.6668038350066379, 2.4580179154988375, 0.4179811001178792, 0.378006739266147, 1.0953331383893428, 0.43554593814249193, 2.5332636651069933, 1.706292489686149, 2.7175347278330357, 2.585867164615448, 1.3187891940596972, 2.689773278381184, 2.341977812171617, 2.587708911681648, 0.4511664853681419];
    double[] knots = Utility.linspace(-6.0, 6.0, 40).ToArray();
    _testBsplineRegression(x, y, knots, 3, 0.0);

    // y = 0.1 * (x - 1.0) * (x - 2.0) * (x + 5.0)
    knots = Utility.linspace(-6.0, 6.0, 19).ToArray();
    x = [3.393423552747411, -3.856513528480578, 4.70120053830043, 3.568431925347083, -5.777431906875562, -5.364092573783452, 3.350427301803606, -0.41783373987170247, 5.789987674408797, -3.8827720164112804, 0.8243881858776518, 3.821696178292248, -0.052360353804902005, 0.3230699419588623, -4.180223617088194, -2.306085079253079, 0.33386881997059703, 2.9852637486551927, -0.7411091737851265, 2.5913706963123335, -2.667634321934398, -3.891769353569082, -1.9778297621400132, -2.526732396655764, 2.9711014160077873, 2.3028608863230886, 0.7990813334970479, -1.8766035755639052, -2.9822077730756624, -5.7721944152989515, 5.964585438891813, 2.8046100325178767, -0.3756968121812818, 0.4771045877238249, 4.495687676165842, 1.192978659145668, -4.033295107211762, -5.533824330396653, -3.6770534889133493, -5.603728638353853, 3.833260358845031, 0.09066065247153077, 3.7389969590961663, 4.183691524348145, -0.8858807553115327, 0.8528286437204455, -5.1965752689624995, -5.847130468753418, 0.48324476473189737, -4.647400995709988, -3.1948584903343744, 5.167213542963484, 4.830660664668333, 3.4274259296691536, 5.626895248612463, 0.3735128169113615, -4.537838607293663, -0.9319709412084425, -1.8887676270109397, -3.8409647774726685, 4.623273787564456, 0.7231506575418258, 3.9795723066000104, -2.1674243306021954, -0.5017368545769729, 5.775113835511508, 0.8044469675167925, 3.4706322255306503, -1.5950061974928573, 3.208506591047655, -4.029503057274278, -1.477849229316611, 4.11798619550086, -5.518281165905134, 5.978450777229707, -2.955051080289382, 5.939328023348821, -2.845029271359729, 4.608794421756162, 5.4114098469404475, 2.1971918257676357, -4.928481379270438, -2.2783523116705635, -4.416145347311105, 3.55926451074715, 1.4048100817382094, 0.39178183072205286, 0.70304596265016, -0.359504357134413, -0.4722537297946108, -1.9169550063884682, 4.75712733332043, -1.709453804085932, 3.8740951278784284, 2.028425045275579, 4.758081800037964, -2.8063777351402, 1.9714187140874806, -1.1787071665646671, -5.624485980612189, -1.1594936271119423, -1.8541981571201829, -0.7671186199034636, -2.680679359459435, -2.5614472817352247, 4.03217760332323, -3.2445326538197965, -1.2463893646169488, -1.2007925181358843, 1.095563175544421, 0.1075920357992377, -4.966556252543567, -5.85481566471616, 5.808526692012688, -5.302819640588166, 2.6098390760242314, -1.0224414361294363, 2.0700694600332366, -1.679593285474743, 4.141516246737577, -5.75084842730176, -0.9895176547337456, 5.5554095303332325, 0.10223662773128162, 2.9987057856586112, 4.348329694484001, 2.449148421197494, 3.479541446068639, -4.699673152800555, -0.9593889575546921, -4.265391405752295, -4.427259646041909, -2.2765921146860593, 2.9590298675613376, 5.126002779851509, -5.013382870189096, -0.4369354541441979, -5.8283653638897714, -2.7532272975327827, 2.5475791308959916];
    y = [0.7794415911017798, 1.9262164067963086, 0.36790246745346566, 0.6610037729620867, 1.6233090017665763, 2.2145553806687652, 0.8127595184281365, 0.6664557656407747, 0.6228529310531857, 1.9643440344007548, 2.0836736089968366, 0.5331920937926987, 0.9490095361094076, 1.3736606330785486, 2.3672060675220425, 0.4763334766768936, 1.3877734963197539, 1.1684673537227528, 0.5091021542225342, 1.6868728665865083, 0.6335528538898689, 1.9773707115778931, 0.3991973847215328, 0.5616679846960785, 1.1849096001836006, 2.1039068543442645, 2.0476970003167683, 0.38534962861817235, 0.8532429841146244, 1.6307528877857627, 0.7310764820441578, 1.3918599277980577, 0.6928644975386651, 1.582821521432305, 0.37658480493132984, 2.5331710446697047, 2.177423213856732, 1.9761906050247178, 1.6656866277284448, 1.874553413765852, 0.5284418503334294, 1.0947614746506198, 0.5697838192792944, 0.4216984398930555, 0.46094688480066404, 2.1236668785785566, 2.4231152731294716, 1.525567951759975, 1.5914647672465339, 2.712549653252494, 1.054683426511396, 0.40724589453761156, 0.37045842512012056, 0.7543061064083619, 0.543252089030102, 1.4403530963840283, 2.67728902058434, 0.4480740909023096, 0.3867906743611465, 1.9035819232664766, 0.36934213539514527, 1.938181335978657, 0.47554531843246894, 0.4372566784709729, 0.6181964152476452, 0.6147785067957978, 2.0553521992509016, 0.7238766504120506, 0.36798726205471516, 0.9353223830755119, 2.1722311844597755, 0.36947081477509075, 0.4367112334031016, 1.9986417894495172, 0.7407886279314544, 0.8307207490674784, 0.7138224089528399, 0.7465928108949714, 0.36985698287474783, 0.4651140681451976, 2.248235817347863, 2.655791563216709, 0.46768854765312473, 2.6024149186590573, 0.6665543825837804, 2.6811770144045397, 1.464971578728001, 1.9089328638264045, 0.7034135331958382, 0.6345149136576269, 0.3903611434902956, 0.36824772305914316, 0.37142718245081885, 0.5123576694195568, 2.4524872244297327, 0.3682636103697108, 0.7196630709737678, 2.5113449507761927, 0.3968833907567873, 1.8442797313069237, 0.39987600136151596, 0.38285086034235793, 0.4995419111548482, 0.6409740662243186, 0.5780207396954538, 0.4595811310719783, 1.1082235118166968, 0.38757731715527693, 0.39363705740318444, 2.4331484287706133, 1.1133623425748176, 2.632339545872219, 1.5149589000150256, 0.6331582871000264, 2.294926919154197, 1.66037845659748, 0.42597132564036877, 2.405916250924209, 0.37006099096730866, 0.4310937480933731, 1.6612132941661713, 0.43354433169974127, 0.5141652574494399, 1.1074483873331487, 1.1530390997172149, 0.39279825890833103, 1.8934875314483401, 0.7178083938569728, 2.7180620777219535, 0.4409424237996799, 2.4639087048479404, 2.6107180052968264, 0.4671543260893906, 1.199074924936303, 0.40024663147608186, 2.5987805162992084, 0.6549706743364261, 1.5516215862787983, 0.6847674339419764, 1.7501325574918662];
    _testBsplineRegression(x, y, knots, 3, 0.0);
}

void _test_bspline(double start, double end, int knotSize, int gridSize, int order)
{
    double[] knots = Utility.linspace(start, end, knotSize).ToArray();
    BSpline splines = new BSpline(knots);

    // foreach B_{i, p} evaluate there values at start to end with gridSize
    // table index by (knot, x)
    List<double[]> table = new List<double[]>();

    // when order = 0 -> knotSize - 1 splines
    // when order = k -> knotSize - 1 - k splines
    for (int i = 0; i < knotSize - 1 - order; ++i)
        table.Add(new double[gridSize]);

    int idx = 0;
    foreach (double x in Utility.linspace(start, end, gridSize))
    {
        double[] splineValues = splines.Eval(x, order);
        for (int i = 0; i < splineValues.Length; ++i)
        {
            table[i][idx] = splineValues[i];
        }
        idx += 1;
    }

    // print values
    Console.WriteLine("grid:");
    double[] xs = Utility.linspace(start, end, gridSize).ToArray();
    Console.WriteLine($"{Utility.ArrToString(xs)}");

    Console.WriteLine("splines:");
    foreach (var values in table)
    {
        Console.WriteLine($"{Utility.ArrToString(values)},");
    }
}

void _testLUFullPivot(double[,] A)
{
    double[,] B = (double[,])A.Clone();
    int[] rowPivot, colPivot;
    int rank = LinearAlg.LUDecompositionFullPivot(B, out rowPivot, out colPivot);

    // extract L, U from B
    int n = A.GetLength(0);
    double[,] L = new double[n, n];
    double[,] U = new double[n, n];

    for (int i = 0; i < n; ++i) L[i, i] = 1.0;
    for (int i = 0; i < n; ++i)
    {
        for (int j = 0; j < n; ++j)
        {
            // lower half
            if (i > j)
            {
                L[i, j] = B[i, j];
            }
            else
            {
                U[i, j] = B[i, j];
            }
        }
    }

    // check if PAQ^T = LU
    // <=> A = P^T (LU) Q
    double[,] mult = LinearAlg.MatMul(L, U);
    for (int i = rank - 1; i >= 0; --i)
    {
        // swap mult[i,:] with mult[pivot[i]:]
        for (int j = 0; j < n; ++j)
        {
            (mult[i, j], mult[rowPivot[i], j]) = (mult[rowPivot[i], j], mult[i, j]);
        }
    }

    for (int j = rank - 1; j >= 0; --j)
    {
        // swap mult[:,j] with mult[:,pivot[j]]
        for (int i = 0; i < n; ++i)
        {
            (mult[i, j], mult[i, colPivot[j]]) = (mult[i, colPivot[j]], mult[i, j]);
        }
    }

    double err = LinearAlg.L2Norm(LinearAlg.MatMinus(A, mult));
    Console.WriteLine($"reconstruct error: {err}, rank={rank}");
}

void _testSolve(double[,] A, double[] b)
{
    double[] x = LinearAlg.Solve(A, b);

    double[] z = new double[x.Length];
    double err = 0;
    for (int i = 0; i < x.Length; ++i)
    {
        for (int j = 0; j < x.Length; ++j)
        {
            z[i] += A[i, j] * x[j];
        }

        err += (z[i] - b[i]) * (z[i] - b[i]);
    }
    err = Math.Sqrt(err);
    Console.WriteLine($"reconstruct error: {err}");
}

void _testLinearRegress(double[,] A, double[] b, double lambda)
{
    double[] x = LinearAlg.LinearRegress(A, b, lambda);

    // check A^T(Ax - b) + lambda x == 0
    int n = A.GetLength(0), p = A.GetLength(1);

    double[] residual = new double[n];
    for (int i = 0; i < n; ++i)
    {
        for (int j = 0; j < p; ++j)
        {
            residual[i] += A[i, j] * x[j];
        }
        residual[i] = residual[i] - b[i];
    }

    double sum = 0.0;
    double[] graident = new double[p];
    for (int i = 0; i < p; ++i)
    {
        graident[i] = lambda * x[i];
        for (int j = 0; j < n; ++j)
            graident[i] += A[j, i] * residual[j];

        sum += graident[i] * graident[i];
    }

    Console.WriteLine($"gradient error: {Math.Sqrt(sum)}");
}

void _testBsplineRegression(double[] x, double[] y, double[] knots, int order, double lambda)
{
    BSplineRegression regr = new BSplineRegression(knots, order, true, lambda);
    regr.Fit(x, y);
    double[] predict = regr.Predict(x);

    double err = LinearAlg.L2Norm(LinearAlg.Minus(y, predict));
    double relErr = err / (LinearAlg.L2Norm(y) + 1e-10);

    Console.WriteLine($"error = {err}, relative error = {relErr}");
    // Console.WriteLine($"{Utility.ArrToString(predict)}");
}