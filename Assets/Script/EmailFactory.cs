using UnityEngine;
using TMPro;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;

public class EmailFactory : MonoBehaviour
{
    [Header("Campos de Entrada (TMP)")]
    public TMP_InputField nomeInput;
    public TMP_InputField cursoInput;
    public TMP_InputField grupoInput;
    public TMP_Text objetivoText;
    public TMP_Text pontuacaoText;
    //public TMP_InputField bodyMessage; // mensagem adicional opcional

    [Header("Configuração SMTP")]
    public string smtpUser = "";   // Ex: seu-email@gmail.com
    public string smtpPass = "";   // Ex: App Password (senha de app do Gmail)

    private const string destinatarioFixo = "reydnermiranda.senai@fieg.com.br";

    // Chame este método no botão "Enviar"
    public void SendEmail()
    {
        Thread t = new Thread(() => SendEmailInternal());
        t.IsBackground = true;
        t.Start();
    }

    private void SendEmailInternal()
    {
        try
        {
            string user = smtpUser;
            string pass = smtpPass;

            // Se não definidos no Inspector, tenta ler das variáveis de ambiente
            if (string.IsNullOrEmpty(user))
                user = Environment.GetEnvironmentVariable("SMTP_USER");
            if (string.IsNullOrEmpty(pass))
                pass = Environment.GetEnvironmentVariable("SMTP_PASS");

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                Debug.LogError("Credenciais SMTP não encontradas. Defina smtpUser/smtpPass ou use variáveis de ambiente SMTP_USER/SMTP_PASS.");
                return;
            }

            // Monta corpo do e-mail
            string corpo = $"Nome: {nomeInput.text}\n" +
                           $"Curso: {cursoInput.text}\n" +
                           $"Grupo: {grupoInput.text}\n" +
                           $"Cenário: Planejamento\n" +
                           $"Objetivo: {objetivoText.text}\n" +
                           $"Pontuação: {pontuacaoText.text}\n\n";

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(user);
            mail.To.Add(destinatarioFixo);
            mail.Subject = $"Envio de Atividade - {nomeInput.text}";
            mail.Body = corpo;

            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Timeout = 10000;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(user, pass);

                smtp.Send(mail);
            }

            Debug.Log("E-mail enviado com sucesso!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Erro ao enviar e-mail: " + ex.Message);
        }
    }
}
