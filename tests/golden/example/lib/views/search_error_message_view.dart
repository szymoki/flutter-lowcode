// Generated from IR
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../presenters/psearch_error_message_presenter.dart';
import '../controllers/search_error_message_controller.dart';

class SearchErrorMessageView extends StatefulWidget {
  final CSearchErrorMessage controller;

  const SearchErrorMessageView({super.key, required this.controller});

  @override
  State<SearchErrorMessageView> createState() => _SearchErrorMessageViewState();
}

class _SearchErrorMessageViewState extends State<SearchErrorMessageView> {
  final _titleController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) context.read<PSearchErrorMessage>().show();
    });
  }

  @override
  void dispose() {
    _titleController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<PSearchErrorMessage>(
      builder: (context, presenter, child) {
        if (!presenter.isVisible) return const SizedBox.shrink();
        return Scaffold(
          appBar: AppBar(title: const Text('SearchErrorMessage')),
          body: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _titleController, decoration: const InputDecoration(labelText: 'Title', border: OutlineInputBorder()),)),
                ElevatedButton(onPressed: () => widget.controller.onClose(), child: const Text('close')),
              ],
            ),
          ),
        );
      },
    );
  }
}
