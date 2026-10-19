// Generated from IR
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../presenters/pdelete_confirmation_message_presenter.dart';
import '../controllers/delete_confirmation_message_controller.dart';

class DeleteConfirmationMessageView extends StatefulWidget {
  final CDeleteConfirmationMessage controller;

  const DeleteConfirmationMessageView({super.key, required this.controller});

  @override
  State<DeleteConfirmationMessageView> createState() => _DeleteConfirmationMessageViewState();
}

class _DeleteConfirmationMessageViewState extends State<DeleteConfirmationMessageView> {


  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) context.read<PDeleteConfirmationMessage>().show();
    });
  }

  @override
  void dispose() {

    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<PDeleteConfirmationMessage>(
      builder: (context, presenter, child) {
        if (!presenter.isVisible) return const SizedBox.shrink();
        return Scaffold(
          appBar: AppBar(title: const Text('DeleteConfirmationMessage')),
          body: Padding(
            padding: const EdgeInsets.all(16.0),
            child: ListView(
              children: [
                ElevatedButton(onPressed: () => widget.controller.onDelete(), child: const Text('delete')),
                ElevatedButton(onPressed: () => widget.controller.onCancel(), child: const Text('cancel')),
              ],
            ),
          ),
        );
      },
    );
  }
}
