using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Syncfusion.UI.Xaml.Charts;
using System.Collections.ObjectModel;
using System.Numerics;

namespace WpfApp1   
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();
            Data = new ObservableCollection<TransmissionData>
            {
                //new TransmissionData() { Resistance = 1.33, Reactance = 2 },

            };
            Data2 = new ObservableCollection<TransmissionData>
            {
                //new TransmissionData() { Resistance = 1.33, Reactance = 2 },

            };

            ImpedanceMarker = new ObservableCollection<TransmissionData>{};
            AdmittanceMarker = new ObservableCollection<TransmissionData>{};
            InputImpedanceMarker = new ObservableCollection<TransmissionData> {};
        }

        public ObservableCollection<TransmissionData> Data2 { get; set; }
        public ObservableCollection<TransmissionData> Data { get; set; }
        public ObservableCollection<TransmissionData> ImpedanceMarker { get; set; }
        public ObservableCollection<TransmissionData> AdmittanceMarker  { get; set; }
        public ObservableCollection<TransmissionData> InputImpedanceMarker { get; set; }


        private void Btn_Clicked(object sender, EventArgs e)
        {
            Data.Clear();
            Data2.Clear();
            ImpedanceMarker.Clear();
            AdmittanceMarker.Clear();
            InputImpedanceMarker.Clear();

            string OutputText = "";
            double Z0 = z0_box.Value;
            Complex ZL = new Complex(zl_real.Value, zl_im.Value);

            if (!((ZL.Real == 0 && ZL.Imaginary ==0) || (Z0 < 0.1)))
            {
                //Complex YL = 1 / ZL;

                Complex zl = ZL / Z0;
                Complex yl = 1 / zl;

                ImpedanceMarker.Add(new TransmissionData() { Resistance = zl.Real, Reactance = zl.Imaginary });
                AdmittanceMarker.Add(new TransmissionData() { Resistance = yl.Real, Reactance = yl.Imaginary });

                double electrical_length = l_box.Value;

                Output output = new Output { };
                Complex Zin = output.Calculate_input_impedance(Z0, ZL, electrical_length);
                Complex zin = Zin / Z0;
                InputImpedanceMarker.Add(new TransmissionData() { Resistance = zin.Real, Reactance = zin.Imaginary });

                Complex reflection_coefficient = output.Calculate_reflection_coefficient(ZL, Z0);
                double SWR = output.Calculate_SWR(reflection_coefficient);

                OutputText += "Frequency of Operation: " + f_box.Value + " MHz\n";
                OutputText += "Input Impedance (denormalised): " + Zin.Real.ToString("+#;-#;0") + Zin.Imaginary.ToString("+#;-#;0") + "j Ohms\n";
                OutputText += "Reflection Coefficient: " + reflection_coefficient.Magnitude.ToString("N3") + "<" + reflection_coefficient.Phase.ToString("N3") + " radians\n";
                OutputText += "Standing Wave Ratio: " + SWR.ToString("N3") + "\n";

                OutputBox.Text = OutputText;

                double step = 0.001;


                for (double i = 0; i <= SWR; i += step)
                {
                    for (double j = 0; j <= Math.PI; j += Math.PI / 300)
                    {
                        Complex impedance = Complex.FromPolarCoordinates(i, j);
                        Complex reflectionCoefficient = (impedance - 1) / (impedance + 1);

                        if (Math.Abs(reflectionCoefficient.Magnitude - reflection_coefficient.Magnitude) < 0.0001)
                        {
                            Data.Add(new TransmissionData() { Resistance = impedance.Real, Reactance = impedance.Imaginary });
                        }
                    }


                }


                for (double i = 0; i <= SWR; i += step)
                {
                    for (double j = Math.PI; j <= 2 * Math.PI; j += Math.PI / 300)
                    {
                        Complex impedance = Complex.FromPolarCoordinates(i, j);
                        Complex reflectionCoefficient = (impedance - 1) / (impedance + 1);

                        if (Math.Abs(reflectionCoefficient.Magnitude - reflection_coefficient.Magnitude) < 0.0001)
                        {
                            Data2.Add(new TransmissionData() { Resistance = impedance.Real, Reactance = impedance.Imaginary });
                        }
                    }
                }


            }

        }
    }

    public class TransmissionData
    {
        public double Resistance { get; set; }

        public double Reactance { get; set; }
    }

    public class Output
    {


        public Complex Calculate_reflection_coefficient(Complex Zl, Complex Z0)
        {
            Complex reflection_coefficient = (Zl - Z0) / (Zl + Z0);
            return reflection_coefficient;
        }

        public Complex Calculate_input_impedance(Complex Z0, Complex Zl, double electrical_length)
        {
            Complex tanBl = Math.Tan(electrical_length);
            Complex Zin = (Z0) * ((Zl + Complex.ImaginaryOne * Z0 * tanBl) / (Z0 + Complex.ImaginaryOne * Zl * tanBl));
            return Zin;
        }

        public double Calculate_SWR(Complex Reflection_coefficient) 
        {
            return (1 + Reflection_coefficient.Magnitude) / (1 - Reflection_coefficient.Magnitude);
        }

    }

}


