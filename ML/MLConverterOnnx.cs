using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Transforms.Onnx;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;

namespace MagicDrive.ML
{
    public class MLConverterOnnx
    {
        public void ConvertModel()
        {

            Stream toto = null;
            // Define the paths
            string mlnetModelPath = @"ML/MLModel.zip";   //"path_to_your_mlnet_model.zip";
            // onnxModelPath = "output_model.onnx";

           // Create a new MLContext
            MLContext mlContext = new MLContext();

            // Load the ML.NET model
            ITransformer mlModel = mlContext.Model.Load(mlnetModelPath, out _);

            mlContext.Model.ConvertToOnnx(mlModel,null,toto);

            var ggg = toto;

        }
    }
}
